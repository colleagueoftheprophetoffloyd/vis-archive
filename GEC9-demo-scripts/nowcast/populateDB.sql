-- --------------------------------
-- ------ Nowcast -----------------
-- --------------------------------

-- Re-create empty database from prototype schema.
DROP DATABASE IF EXISTS nowcast;
CREATE DATABASE nowcast;
USE nowcast;

CREATE TABLE nodes LIKE	prototypeSchema.nodes;
CREATE TABLE visuals LIKE prototypeSchema.visuals;
CREATE TABLE status LIKE prototypeSchema.status;
CREATE TABLE links LIKE prototypeSchema.links;
CREATE TABLE statistics LIKE prototypeSchema.statistics;

--
-- Map settings
--
INSERT into visuals
VALUES (
       1,			-- as id,
       'nowcast',		-- as slicename,
       'mainMap',		-- as name,
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
       'centerLon=-77,',
       'zoomLevel=7,',
       'mapMode=road,'
       'leftimage=images/umassHorizWhite.png,',
       'rightimage=images/casa.png'
       )			-- as renderAttributes
       );


-- ----------------------------------
-- -- Places
-- ----------------------------------

INSERT INTO nodes
VALUES
	(100, 'Sensor1', 42.3803676, -72.523143, NULL, 'images/radar.jpg'),
	(200, 'AmazonEast', 38.9, -77.1, NULL, 'images/ec2White.png'),
	(300, 'RENCI', 35.9413852, -79.0182061, NULL, 'images/renci.jpg'),
	(400, 'Server1', 42.3803676, -74, NULL, 'images/UMassSealWhite.png'),
	(500, 'Starlight', 41.883507, -87.639608, NULL, 'images/starlightWhite.png');

-- Icons for each location
INSERT into visuals
SELECT id as id,
       'nowcast' as slicename,
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
       name as statusHandle,
       'height=100,width=100' as renderAttributes
FROM   nodes
WHERE  name in ('Sensor1', 'AmazonEast', 'RENCI', 'Server1', 'Starlight');

UPDATE visuals
SET    renderAttributes = 'width=200,height=100'
WHERE  statusHandle in ('AmazonEast', 'Starlight');

UPDATE visuals
SET    renderAttributes = 'width=100,height=150'
WHERE  statusHandle in ('Sensor1');


-- -------------------------------------
-- - Links
-- -------------------------------------

-- Insert links
INSERT INTO links
VALUES (1000, 'fixme', 'Sensor1', 'AmazonEast'),
       (1100, 'fixme', 'AmazonEast', 'RENCI'),
       (1200, 'fixme', 'RENCI', 'Server1');
-- Fix names
UPDATE links
SET    name = CONCAT(sourceNode, '-', destNode)
WHERE  name = 'fixme';


-- Insert arcs for links
INSERT INTO visuals
SELECT id as id,
       'nowcast' as slicename,
       name as name,
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
       name as statusHandle,
       CONCAT('datapath=', sourceNode, ':', destNode) as renderAttributes
FROM   links
WHERE  id in (1000, 1100, 1200);

UPDATE visuals
SET    renderAttributes='datapath=RENCI:Starlight:Server1'
WHERE  name = 'RENCI-Server1';


INSERT into visuals
SELECT id+1 as id,
       'nowcast' as slicename,
       'ec2-bytes' as name,
       NULL as subSlice,
       2 as sequence,
       'lineGraph' as infoType,
       'link' as objType,
       name as objName,
       'MB' as statType,
       40 as statHistory,
       NULL as minValue,
       NULL as maxValue,
       'select time, value from statistics where stat=''ec2-bytes'' order by time desc limit 40' as statQuery,
       name as statusHandle,
       'alignment=left' as renderAttributes
FROM   links
WHERE  name = 'Sensor1-AmazonEast';

-- EC2 budget
INSERT into visuals
SELECT id+2 as id,
       'nowcast' as slicename,
       'ec2-cost' as name,
       NULL as subSlice,
       3 as sequence,
       'scalarText' as infoType,
       'link' as objType,
       name as objName,
       'EC2 budget $' as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       'select time, round(value,2) as value from statistics where stat=''ec2-cost'' order by time desc limit 1' as statQuery,
       name as statusHandle,
       'stringformat={0:0.00}' as renderAttributes
FROM   links
WHERE  name = 'Sensor1-AmazonEast';



-- Status items for everything
INSERT into status
SELECT NULL as id,
       'nowcast' as sliceName,
       name as handle,
       'normal' as status
FROM   nodes;

INSERT into status
SELECT NULL as id,
       'nowcast' as sliceName,
       name as handle,
       'forward' as status
FROM   links;

