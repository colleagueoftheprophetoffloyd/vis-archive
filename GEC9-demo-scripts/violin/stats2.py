import MySQLdb
import time
import socket
import random

dbHost='bain.gpolab.bbn.com'
dbUser='violin'
dbPassword='vi0lin'
dbName='violin'

reportingInterval=2
tableName='statistics'
myNode=socket.gethostname()
nodesToReport=['Utah-A', 'Utah-B', 'Utah-C',
               'Utah-D', 'Utah-E', 'Utah-F',
               'GPO-A', 'GPO-B', 'GPO-C',
               'GPO-D', 'GPO-E', 'GPO-F']
highUsageNodes=nodesToReport[6:]
linksToReport=['Utah-GPO']
PGSitesToReport=['Utah','GPO']
highUsagePGSites=['Utah']

timeFormat='%Y-%m-%d %H:%M:%S';

conn = MySQLdb.connect(host=dbHost,
                       user=dbUser,
                       passwd=dbPassword,
                       db=dbName);

def reportLoad():
    values = []
    nowString = time.strftime(timeFormat, time.gmtime())
    for node in nodesToReport:
        load = 0.4*random.random()
        if node in highUsageNodes:
            load += 0.6;
        values.append(str.format("(NULL, '{0}-load','{1}', {2})",
                                 node, nowString, load))
    sql = str.format("insert into {0} values {1}",
                     tableName, ','.join(values));
    conn.query(sql)

def reportNet():
    values = []
    nowString = time.strftime(timeFormat, time.gmtime())
    for link in linksToReport:
        bw = 15000.0 + 5000.0 * random.random()
        values.append(str.format("(NULL, '{0}-BWUsage','{1}', {2})",
                                 link, nowString, bw))
    sql = str.format("insert into {0} values {1}",
                     tableName, ','.join(values));
    conn.query(sql)

def reportPG():
    values = []
    nowString = time.strftime(timeFormat, time.gmtime())
    for site in PGSitesToReport:
        nodes = int(5 + 5*random.random())
        if site in highUsagePGSites:
            nodes += 20
        values.append(str.format("(NULL, '{0}-PGUsage','{1}', {2})",
                                 site, nowString, nodes))
    sql = str.format("insert into {0} values {1}",
                     tableName, ','.join(values));
    conn.query(sql)


while True:
    reportLoad()
    reportNet()
    reportPG()
    time.sleep(reportingInterval)

