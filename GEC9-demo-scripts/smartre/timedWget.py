import MySQLdb
import time
import socket
from subprocess import *

dbHost='bain.gpolab.bbn.com'
dbUser='smartre'
dbPassword='sm@rtre'
dbName='smartre'
tableName='statistics'
myNode=socket.gethostname()

timeFormat='%Y-%m-%d %H:%M:%S';

conn = MySQLdb.connect(host=dbHost,
                       user=dbUser,
                       passwd=dbPassword,
                       db=dbName);

def timedWget(url, destFile):
    start = time.time()
    err = Popen(["wget", "-O", destFile, url], stderr=PIPE).communicate()[1]
    elapsed = time.time() - start
    errLines = err.split('\n')
    size = float(errLines[-3].split('[')[1].split('/')[0].split(']')[0])
    return (elapsed, size)

def wgetAndReport(url, destFile):
    elapsed, size = timedWget(url, destFile)
    # Probably don't want both of these.
    # Report latency
    sql = str.format("insert into {0} set stat='{1}-latency', time='{2}', value='{3}'",
                     tableName,
                     myNode, 
                     time.strftime(timeFormat, time.gmtime()),
                     elapsed);
    conn.query(sql)
    # Report throughput
    sql = str.format("insert into {0} set stat='{1}-throughput', time='{2}', value='{3}'",
                     tableName,
                     myNode, 
                     time.strftime(timeFormat, time.gmtime()),
                     size/elapsed);
    conn.query(sql)


wgetAndReport('http://www.wisc.edu', '/dev/null')
time.sleep(3)
wgetAndReport('http://www.google.com', '/dev/null')
time.sleep(3)
wgetAndReport('http://www.bbn.com', '/dev/null')

