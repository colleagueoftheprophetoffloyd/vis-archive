-- --------------------------------
-- -------- CDN Emily
-- --------------------------------

-- Re-create empty database from prototype schema.
DROP DATABASE IF EXISTS cdnemily;
CREATE DATABASE cdnemily;
USE cdnemily;

CREATE TABLE nodes LIKE	prototypeSchema.nodes;
CREATE TABLE visuals LIKE prototypeSchema.visuals;
CREATE TABLE status LIKE prototypeSchema.status;
CREATE TABLE links LIKE prototypeSchema.links;


INSERT INTO nodes
SELECT NULL as id,
       hostname as name,
       lat as latitude,
       lon as longitude,
       NULL as type,
       NULL as icon
FROM   jkarlin.cdn_host_pool
WHERE  experiment = 'Emily';

UPDATE nodes
SET    icon = 'images/server1.png'
WHERE  name like '%.emulab.net';

INSERT INTO nodes
SELECT NULL as id,
       CONCAT(name, '-anchor') as name,
       latitude as latitude,
       longitude as longitude,
       NULL as type,
       NULL as icon
FROM   nodes
WHERE  ((name like '%rice.edu')
	OR
	(name like '%umich.edu'));

INSERT INTO nodes
SELECT DISTINCT
       NULL as id,
       'Utah-anchor' as name,
       latitude as latitude,
       longitude as longitude,
       NULL as type,
       NULL as icon
FROM   nodes
WHERE  name like '%emulab.net'
AND    name not like '%uky.emulab.net';

INSERT INTO nodes
SELECT DISTINCT
       NULL as id,
       'Kentucky-anchor' as name,
       latitude as latitude,
       longitude as longitude,
       NULL as type,
       NULL as icon
FROM   nodes
WHERE  name like '%uky.emulab.net';

-- Williams
INSERT INTO nodes
VALUES (900, 'Williams', 42.71, -73.21, NULL, 'images/WilliamsWhite.png');


--
-- Map settings
--
INSERT into visuals
VALUES (
       1,			-- as id,
       'Emily',			-- as sliceName,
       'mainMap',		-- as name
       NULL,			-- as subSlice,
       0,			--  as sequence,
       'map',			--  as infoType,
       '',			--  as objType,
       '',			--  as objName,
       NULL,			--  as statType,
       NULL,			--  as statHistory,
       NULL,			--  as minValue,
       NULL,			--  as maxValue,
       NULL,			--  as statQuery,
       NULL,			-- as statusHandle,
       CONCAT(
       'centerLat=40,',
       'centerLon=-85,',
       'zoomLevel=4,',
       'mapMode=road,',
       'leftimage=images/WilliamsWhite.png,',
       'rightimage=images/GushWhite.png'
       )			-- as renderAttributes
       );


INSERT into visuals
SELECT id as id,
       'Emily' as sliceName,
       name as name,
       NULL as subSlice,
       1 as sequence,
       'icon' as infoType,
       'node' as objType,
       name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       NULL as statusHandle,
       'width=100,height=100,alignment=bottomright' as renderAttributes
FROM   nodes
WHERE  name = 'Williams';

-- Make a visual entry for each host.
INSERT into visuals
SELECT id+1000 as id,
       experiment as slicename,
       hostname as name,
       NULL as subSlice,
       0 as sequence,
       'point' as infoType,
       'node' as objType,
       hostname as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       hostname as statusHandle,
       NULL as renderAttributes
FROM   jkarlin.cdn_hosts
WHERE  experiment = 'Emily';

UPDATE visuals
SET    infoType = 'icon',
       renderAttributes = 'width=100,height=100'
WHERE  name like '%.emulab.net';

-- Make links to match topology.
INSERT INTO links
SELECT NULL as id,
       CONCAT((select hostname from jkarlin.cdn_hosts where host_num=t.neighbor_host_num),
       	      '-',
       	      h.hostname) as name,
       (select hostname from jkarlin.cdn_hosts where host_num=t.neighbor_host_num) as sourceNode,
       h.hostname as destNode
FROM   jkarlin.cdn_topology t, jkarlin.cdn_hosts h
WHERE  t.host_num = h.host_num
AND    neighbor_num > -1
AND    t.experiment = 'Emily';

-- Make a visual for each link.
INSERT INTO visuals
SELECT NULL as id,
       t.experiment as slicename,
       CONCAT((select hostname from jkarlin.cdn_hosts where host_num=t.neighbor_host_num),
       	      '-',
       	      h.hostname) as name,
       NULL as subSlice,
       0 as sequence,
       'arc' as infoType,
       'link' as objType,
       CONCAT((select hostname from jkarlin.cdn_hosts where host_num=t.neighbor_host_num),
       	      '-',
       	      h.hostname) as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       CONCAT((select hostname from jkarlin.cdn_hosts where host_num=t.neighbor_host_num),
       	      '-',
       	      h.hostname) as statusHandle,
       CONCAT('datapath=',
	      (select hostname from jkarlin.cdn_hosts where host_num=t.neighbor_host_num),
       	      ':',
       	      h.hostname) as renderAttributes
FROM   jkarlin.cdn_topology t, jkarlin.cdn_hosts h
WHERE  t.host_num = h.host_num
AND    neighbor_num > -1
AND    t.experiment = 'Emily';


-- Client latency visuals
INSERT INTO visuals
SELECT NULL as id,
       t.experiment as slicename,
       CONCAT(h.hostname, '-latency') as name,
       NULL as subSlice,
       1 as sequence,
       'lineGraph' as infoType,
       'node' as objType,
       CONCAT(h.hostname, '-anchor') as objName,
       'latency' as statType,
       40 as statHistory,
       0 as minValue,
       80 as maxValue,
       CONCAT('select time, latency as value from jkarlin.cdn_client '
       	      'where experiment=''', t.experiment,
	      ''' and topo_num=''', t.topo_num, ''' ',
	      'order by time desc limit 40') as statQuery,
       CONCAT(h.hostname, '-latency') as statusHandle,
       'alignment=top' as renderAttributes
FROM   jkarlin.cdn_topology t, jkarlin.cdn_hosts h
WHERE  t.host_num = h.host_num
AND    ((h.hostname like '%rice.edu')
	OR
	(h.hostname like '%umich.edu'))
AND    t.type = 'client'
AND    t.experiment = 'Emily';

-- Move things around to look pretty.
UPDATE visuals
SET    renderAttributes = 'xoffset=200',
       statType = 'latency\n@Rice'
WHERE  name like '%rice.edu-latency';

UPDATE visuals
SET    renderAttributes = 'alignment=bottom,yoffset=-50',
       statType = 'latency\n@Michigan'
WHERE  name like '%umich.edu-latency';

-- Node efficiency
INSERT INTO visuals
SELECT NULL as id,
       t.experiment as slicename,
       CONCAT(h.hostname, '-efficiency') as name,
       NULL as subSlice,
       1 as sequence,
       'lineGraph' as infoType,
       'node' as objType,
       'Utah-anchor' as objName,
       'pc405\nEfficiency' as statType,
       40 as statHistory,
       0 as minValue,
       5 as maxValue,
       CONCAT('select time, client_bytes_sent/server_bytes_recv as value from jkarlin.cdn_node '
       	      'where experiment=''', t.experiment,
	      ''' and topo_num=''', t.topo_num, ''' ',
	      'order by time desc limit 40') as statQuery,
       CONCAT(h.hostname, '-efficiency') as statusHandle,
       'alignment=bottomleft,yoffset=-50' as renderAttributes
FROM   jkarlin.cdn_topology t, jkarlin.cdn_hosts h
WHERE  t.host_num = h.host_num
AND    t.type = 'cdn_node'
AND    t.experiment = 'Emily';

-- Move one of the nodes graphs down.
UPDATE visuals
SET    sequence = 3,
       statType = 'pc484\nEfficiency'
WHERE  name like 'pc484%-efficiency';


INSERT INTO status
SELECT NULL as id,
       sliceName as sliceName,
       statusHandle as handle,
       IF(infoType='arc',
	  'forward',
	  IF(infoType='point',
	     'active',
	     'normal')) as status
FROM visuals;
