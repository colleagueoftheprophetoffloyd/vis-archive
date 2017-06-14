-- --------------------------------
-- -------- Netserv ---------------
-- --------------------------------

-- Re-create empty database from prototype schema.
DROP DATABASE IF EXISTS	netserv;
CREATE DATABASE netserv;
USE netserv;

CREATE TABLE nodes LIKE	prototypeSchema.nodes;
CREATE TABLE visuals LIKE prototypeSchema.visuals;
CREATE TABLE status LIKE prototypeSchema.status;
CREATE TABLE links LIKE prototypeSchema.links;

--
-- Map settings
--
INSERT into visuals
VALUES (
       1,			-- as id,
       'netserv',		-- as slicename,
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
       'leftimage=images/ColumbiaWhite.png,',
       'rightimage=images/irtWhite.png'
       )			-- as renderAttributes
       );


-- Insert Kansas City Server site
INSERT INTO nodes
VALUES (10, 'KansasCity', 39.07656, -94.55518, NULL, NULL);

-- Insert UUtah ProtoGENI site
INSERT INTO nodes
VALUES (20, 'UUtah', 40.768652, -111.82755, NULL, 'images/UUtah.jpg');

-- Insert GPO ProtoGENI site
INSERT INTO nodes
VALUES (30, 'GPO', 42.39884, -71.1474, NULL, 'images/GENI.jpg');

-- Insert backbone links
INSERT INTO links
VALUES (10, 'KansasCity-GPO', 'KansasCity', 'GPO');
INSERT INTO links
VALUES (20, 'KansasCity-UUtah', 'KansasCity', 'UUtah');


-- Insert GPO detail topology

-- GPO nodes
INSERT INTO nodes
VALUES (3020, 'Client1A', 42.39884, -71.1474, NULL, 'images/phone.png');
INSERT INTO nodes
VALUES (3030, 'Client1B', 42.39884, -71.1474, NULL, 'images/phone.png');

-- GPO internal links
INSERT INTO links
VALUES (3020, 'GPO-Client1A', 'GPO', 'Client1A');
INSERT INTO links
VALUES (3030, 'GPO-Client1B', 'GPO', 'Client1B');


-- 
-- Visuals
--

-- Make visuals for sites
INSERT into visuals
SELECT id as id,
       'netserv' as slicename,
       name as name,
       NULL as subSlice,
       1 as sequence,
       'label' as infoType,
       'node' as objType,
       name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       name as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  id in (10, 20, 30);

-- Make visuals for backbone links
INSERT into visuals
SELECT id+100 as id,
       'netserv' as slicename,
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
WHERE  id in (10, 20);

INSERT into visuals
VALUES (3011, 'netserv', 'GPO-Module1', NULL, 2, 'label', 'node',
        'GPO', NULL, NULL, NULL, NULL, NULL, 'GPO-Module1',
	'text=Module1,background=Green'),
       (3012, 'netserv', 'GPO-Module2', NULL, 3, 'label', 'node',
	'GPO', NULL, NULL, NULL, NULL, NULL, 'GPO-Module2',
	'text=Module2,background=Blue'),
       (3013, 'netserv', 'GPO-Module3', NULL, 4, 'label', 'node',
	'GPO', NULL, NULL, NULL, NULL, NULL, 'GPO-Module3',
	'text=Module3,background=Orange');

-- Visuals for GPO detail nodes
INSERT into visuals
SELECT id as id,
       'netserv' as slicename,
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
       NULL as renderAttributes
FROM   nodes
WHERE  id in (3020, 3030);

-- Move clients a bit.
UPDATE visuals
SET    renderAttributes = 'xoffset=100,yoffset=-50'
WHERE  id = 3020;
UPDATE visuals
SET    renderAttributes = 'xoffset=100,yoffset=50'
WHERE  id = 3030;

-- Make visuals for internal links
INSERT into visuals
VALUES
	(3120, 'netserv', 'GPO-Client1A', NULL, 1,
	 'arc', 'link', 'GPO-Client1A', NULL, NULL,
	 NULL, NULL, NULL, 'GPO-Client1A',
	 'datapath=GPO:Client1A'),
	(3130, 'netserv', 'GPO-Client1B', NULL, 1,
	 'arc', 'link', 'GPO-Client1B', NULL, NULL,
	 NULL, NULL, NULL, 'GPO-Client1B',
	 'datapath=GPO:Client1B');

--
-- Status entries
--
INSERT into status
SELECT id as id,
       'netserv' as sliceName,
       statusHandle as handle,
       'normal' as status
FROM   visuals
WHERE  statusHandle is not NULL;
