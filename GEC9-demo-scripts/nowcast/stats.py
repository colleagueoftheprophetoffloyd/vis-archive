import MySQLdb
import time
import socket
import random

dbHost='bain.gpolab.bbn.com'
dbUser='nowcast'
dbPassword='nowc@st'
dbName='nowcast'

reportingInterval=2
tableName='statistics'
myNode=socket.gethostname()
cloudsToReport=['ec2'];
linksToReport=['ec2']

timeFormat='%Y-%m-%d %H:%M:%S';

conn = MySQLdb.connect(host=dbHost,
                       user=dbUser,
                       passwd=dbPassword,
                       db=dbName);

def reportCost():
    values = []
    nowString = time.strftime(timeFormat, time.gmtime())
    for cloud in cloudsToReport:
        cloudCosts[cloud] += round(2.0*random.random(), 2)
        cost = cloudCosts[cloud]
        values.append(str.format("(NULL, '{0}-cost','{1}', {2})",
                                 cloud, nowString, cost))
    sql = str.format("insert into {0} values {1}",
                     tableName, ','.join(values))
    conn.query(sql)

def reportUpload():
    values = []
    nowString = time.strftime(timeFormat, time.gmtime())
    for link in linksToReport:
        bytes = round(1000*random.random())
        values.append(str.format("(NULL, '{0}-bytes','{1}', {2})",
                                 link, nowString, bytes))
    sql = str.format("insert into {0} values {1}",
                     tableName, ','.join(values));
    conn.query(sql)

cloudCosts={}
for cloud in cloudsToReport:
    cloudCosts[cloud] = 0;
while True:
    reportCost()
    reportUpload()
    time.sleep(reportingInterval)

