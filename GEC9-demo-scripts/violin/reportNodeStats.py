import MySQLdb
import time
import socket

dbHost='bain.gpolab.bbn.com'
dbUser='violin'
dbPassword='vi0lin'
dbName='violin'

reportingInterval=5
tableName='statistics'
myNode=socket.gethostname()
interfacesToReport=['eth0']

timeFormat='%Y-%m-%d %H:%M:%S';

conn = MySQLdb.connect(host=dbHost,
                       user=dbUser,
                       passwd=dbPassword,
                       db=dbName);

def reportLoad():
    with open('/proc/loadavg', 'r') as f:
        l = f.readline()
    load = float(l.split()[0]);
    sql = str.format("insert into {0} set stat='{1}-CPU', time='{2}', value='{3}'",
                     tableName,
                     myNode,
                     time.strftime(timeFormat, time.gmtime()),
                     load)
    conn.query(sql)

def reportNet():
    with open('/proc/net/dev', 'r') as f:
        lines = f.read()
    for line in lines.split('\n'):
        thisIF = line.split(':')[0].strip()
        if thisIF in interfacesToReport:
            rx = line.split()[1]
            tx = line.split()[9]
            sql = str.format("insert into {0} set stat='{1}-{2}-{3}', time='{4}', value='{5}'",
                     tableName,
                     myNode,
                     thisIF,
                     'RX',
                     time.strftime(timeFormat, time.gmtime()),
                     rx)
            conn.query(sql)
            sql = str.format("insert into {0} set stat='{1}-{2}-{3}', time='{4}', value='{5}'",
                     tableName,
                     myNode,
                     thisIF,
                     'TX',
                     time.strftime(timeFormat, time.gmtime()),
                     tx)
            conn.query(sql)
    

while True:
    reportLoad()
    reportNet()
    time.sleep(reportingInterval)

