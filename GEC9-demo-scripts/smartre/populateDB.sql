-- --------------------------------
-- ------ SmartRE -----------------
-- --------------------------------

-- Re-create empty database from prototype schema.
DROP DATABASE IF EXISTS smartre;
CREATE DATABASE smartre;
USE smartre;

CREATE TABLE nodes LIKE	prototypeSchema.nodes;
CREATE TABLE visuals LIKE prototypeSchema.visuals;
CREATE TABLE status LIKE prototypeSchema.status;
CREATE TABLE links LIKE prototypeSchema.links;
CREATE TABLE statistics LIKE prototypeSchema.statistics;
CREATE TABLE locations LIKE prototypeSchema.locations;
INSERT INTO locations (select * from prototypeSchema.locations);

--
-- Map settings
--
INSERT into visuals
VALUES (
       1,			-- as id,
       'smartre',		-- as slicename,
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
       'mapMode=road,'
       'leftimage=images/WiscLogoWhite.png,',
       'rightimage=images/CMURed.png'
       )			-- as renderAttributes
       );


-- --------------------------------------
-- SITES Section
-- --------------------------------------

-- Insert sites
INSERT INTO nodes
VALUES (100, 'Server-switch', 0, 0, NULL, NULL),
       (110, 'Server', 0, 0, NULL, NULL),
       (200, 'Encoder1-switch', 0, 0, NULL, NULL),
       (210, 'Encoder1', 0, 0, NULL, NULL),
       (300, 'Client1-switch', 0, 0, NULL, NULL),
       (310, 'Client1', 0, 0, NULL, NULL),
       (320, 'Client1-decoder', 0, 0, NULL, NULL),
       (400, 'Indiana-switch', 0, 0, NULL, NULL),
       (500, 'Client2-switch', 0, 0, NULL, NULL),
       (510, 'Client2', 0, 0, NULL, NULL),
       (520, 'Client2-decoder', 0, 0, NULL, NULL),
       (900, 'Wisconsin', 43.0762, -89.4123, NULL, 'images/WiscLogoVertWhite.png'),
       (999, 'AtlanticOcean', 41, -70, NULL, NULL);


-- Server at Stanford
UPDATE nodes
SET    latitude = (SELECT latitude
       		   FROM locations 
		   WHERE cityName = 'Stanford'
		   LIMIT 1),
       longitude = (SELECT longitude
       		    FROM locations
		    WHERE cityName = 'Stanford'
		    LIMIT 1)
WHERE  name LIKE 'Server%';

UPDATE nodes
SET    icon='images/Stanford.jpg'
WHERE  name = 'Server-switch';

UPDATE nodes
SET    icon='images/Camera.PNG'
WHERE  name = 'Server';

-- Encoder1 at GaTech
UPDATE nodes
SET    latitude = (SELECT latitude
       		   FROM locations 
		   WHERE cityName = 'Georgia Tech'
		   LIMIT 1),
       longitude = (SELECT longitude
       		    FROM locations
		    WHERE cityName = 'Georgia Tech'
		    LIMIT 1)
WHERE  name LIKE 'Encoder1%';

UPDATE nodes
SET    icon='images/GaTech.jpg'
WHERE  name = 'Encoder1-switch';

-- Client1 at Clemson
UPDATE nodes
SET    latitude = (SELECT latitude
       		   FROM locations 
		   WHERE cityName = 'Clemson'
		   LIMIT 1),
       longitude = (SELECT longitude
       		    FROM locations
		    WHERE cityName = 'Clemson'
		    LIMIT 1)
WHERE  name LIKE 'Client1%';

UPDATE nodes
SET    icon='images/Clemson.png'
WHERE  name = 'Client1-switch';

-- Switch at Indiana
UPDATE nodes
SET    latitude = (SELECT latitude
       		   FROM locations 
		   WHERE cityName = 'Indiana University'
		   LIMIT 1),
       longitude = (SELECT longitude
       		    FROM locations
		    WHERE cityName = 'Indiana University'
		    LIMIT 1)
WHERE  name LIKE 'Indiana%';

UPDATE nodes
SET    icon='images/Indiana.jpg'
WHERE  name = 'Indiana-switch';

-- Client2 at GPO
UPDATE nodes
SET    latitude = (SELECT latitude
       		   FROM locations 
		   WHERE cityName = 'BBN'
		   LIMIT 1),
       longitude = (SELECT longitude
       		    FROM locations
		    WHERE cityName = 'BBN'
		    LIMIT 1)
WHERE  name LIKE 'Client2%';

UPDATE nodes
SET    icon='images/GENI.jpg'
WHERE  name = 'Client2-switch';


-- Insert visuals for switches,
-- server, clients.
INSERT INTO visuals
SELECT id as id,
       'smartre' as slicename,
       nodes.name as name,
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
       name as statusHandle,
       'width=100,height=100' as renderAttributes
FROM   nodes
WHERE  name like '%-switch'
OR     name = 'Server'
OR     name = 'Wisconsin';


INSERT INTO visuals
SELECT id+1 as id,
       'smartre' as slicename,
       nodes.name as name,
       NULL as subSlice,
       2 as sequence,
       'scalar' as infoType,
       'node' as objType,
       name as objName,
       nodes.name as statType,
       NULL as statHistory,
       0 as minValue,
       100 as maxValue,
       CONCAT('select time, value from statistics where stat=''',
       	      nodes.name,
	      '-progress'' order by time desc limit 1') as statQuery,
       name as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  name in ('Client1', 'Client2');

-- Move things around so we can see them
UPDATE	visuals
SET	renderAttributes='xoffset=-100,yoffset=-50,alignment=right'
WHERE	name = 'Server';

UPDATE	visuals
SET	renderAttributes='xoffset=50,yoffset=150,alignment=left'
WHERE	name in ('Client1');

UPDATE	visuals
SET	renderAttributes='xoffset=50,yoffset=-40,alignment=left'
WHERE	name in ('Client2');

UPDATE visuals
SET    renderAttributes = 'width=100,height=100,alignment=left'
WHERE  name = 'Client1-switch';

UPDATE visuals
SET    renderAttributes = 'width=100,height=100,alignment=right'
WHERE  name = 'Encoder1-switch';


-- --------------------------------------
-- End SITES Section
-- --------------------------------------


-- --------------------------------------
-- LINKS Section
-- --------------------------------------

-- Insert links
INSERT INTO links
VALUES (1100, 'fixme', 'Server-switch', 'Encoder1-switch'),
       (1110, 'fixme', 'Server-switch', 'Indiana-switch'),
       (1120, 'fixme', 'Server', 'Server-switch'),
       (1200, 'fixme', 'Encoder1-switch', 'Client1-switch'),
       (1210, 'fixme', 'Encoder1', 'Encoder1-switch'),
       (1300, 'fixme', 'Client1-switch', 'Client2-switch'),
       (1310, 'fixme', 'Client1', 'Client1-switch'),
       (1320, 'fixme', 'Client1-decoder', 'Client1-switch'),
       (1400, 'fixme', 'Indiana-switch', 'Client2-switch'),
       (1510, 'fixme', 'Client2', 'Client2-switch'),
       (1520, 'fixme', 'Client2-decoder', 'Client2-switch');

-- Fix names
UPDATE links
SET    name = CONCAT(sourceNode, '-', destNode)
WHERE  name = 'fixme';


-- Insert arcs for links
INSERT INTO visuals
SELECT id as id,
       'smartre' as slicename,
       id as name,
       NULL as subSlice,
       1 as sequence,
       'arc' as infoType,
       'link' as objType,
       name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       NULL as statusHandle,
       NULL as renderAttributes
FROM   links
WHERE  id in (1100, 1110, 1120, 1300, 1310, 1510);

UPDATE visuals
SET    name = 'south1',
       statusHandle = 'south1',
       renderAttributes = 'datapath=Server-switch:Encoder1-switch:Client1-switch'
WHERE  id=1100;

UPDATE visuals
SET    name = 'south2',
       statusHandle = 'south2',
       renderAttributes = 'datapath=Client1-switch:Client2-switch'
WHERE  id=1300;

UPDATE visuals
SET    name = 'north',
       statusHandle = 'north',
       renderAttributes = 'datapath=Server-switch:Indiana-switch:Client2-switch'
WHERE  id=1110;

UPDATE visuals
SET    name = 'serverLink',
       statusHandle = 'serverLink',
       renderAttributes = 'datapath=Server:Server-switch'
WHERE  id=1120;

UPDATE visuals
SET    name = 'client1Link',
       statusHandle = 'client1Link',
       renderAttributes = 'datapath=Client1:Client1-switch'
WHERE  id=1310;

UPDATE visuals
SET    name = 'client2Link',
       statusHandle = 'client2Link',
       renderAttributes = 'datapath=Client2:Client2-switch'
WHERE  id=1510;



-- --------------------------------------
-- End LINKS Section
-- --------------------------------------


-- --------------------------------------
-- Statistics
-- --------------------------------------


-- Line graphs for statistics.
INSERT INTO visuals
SELECT id+1 as id,
       'smartre' as slicename,
       CONCAT(links.name, '-graph') as name,
       NULL as subSlice,
       1 as sequence,
       'lineGraph' as infoType,
       'link' as objType,
       name as objName,
       'Kbps' as statType,
       40 as statHistory,
       0 as minValue,
       8000 as maxValue,
       NULL as statQuery,
       CONCAT(links.name, '-graph') as statusHandle,
       NULL as renderAttributes
FROM   links
WHERE  id in (1120, 1100, 1510, 1310, 1110);

UPDATE visuals
SET statQuery = CONCAT('select timestamp as time, data_value as value ',
    	      	       'from GEC9_demos.GeniMeasurementDataItem ',
		       'where data_source=120 ',
		       'and data_type=2 ',
		       'order by time desc ',
		       'limit 40'),
    statType = 'Kbps',
    renderAttributes = 'alignment=top,yoffset=80'
WHERE id = 1121;

UPDATE visuals
SET statQuery = CONCAT('select timestamp as time, data_value as value ',
    	      	       'from GEC9_demos.GeniMeasurementDataItem ',
		       'where data_source=48 ',
		       'and data_type=1 ',
		       'order by time desc ',
		       'limit 40'),
    statType = 'Kbps',
    renderAttributes = 'alignment=topright,xoffset=150,yoffset=20'
WHERE id = 1101;

UPDATE visuals
SET statQuery = CONCAT('select timestamp as time, data_value as value ',
    	      	       'from GEC9_demos.GeniMeasurementDataItem ',
		       'where data_source=106 ',
		       'and data_type=1 ',
		       'order by time desc ',
		       'limit 40'),
    statType = 'Kbps',
    renderAttributes = 'alignment=bottomleft,yoffset=-100'
WHERE id = 1511;

UPDATE visuals
SET statQuery = CONCAT('select timestamp as time, data_value as value ',
    	      	       'from GEC9_demos.GeniMeasurementDataItem ',
		       'where data_source=121 ',
		       'and data_type=1 ',
		       'order by time desc ',
		       'limit 40'),
    statType = 'Kbps',
    renderAttributes = 'alignment=left,xoffset=150,yoffset=20'
WHERE id = 1311;

UPDATE visuals
SET statQuery = CONCAT('select timestamp as time, data_value as value ',
    	      	       'from GEC9_demos.GeniMeasurementDataItem ',
		       'where data_source=54 ',
		       'and data_type=1 ',
		       'order by time desc ',
		       'limit 40'),
    statType = 'Kbps',
    renderAttributes = 'alignment=bottomright'
WHERE id = 1111;



-- Status items for everything
INSERT into status
SELECT NULL as id,
       'smartre' as sliceName,
       statusHandle as handle,
       'normal' as status
FROM   visuals;

UPDATE status
SET    status = 'forward'
WHERE  handle = 'south'
OR     handle = 'serverLink';
