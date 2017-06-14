-- --------------------------------
-- ------ Pathlet -----------------
-- --------------------------------

-- Re-create empty database from prototype schema.
DROP DATABASE IF EXISTS pathlet;
CREATE DATABASE pathlet;
USE pathlet;

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
       'pathlet',		-- as slicename,
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
       'zoomLevel=4,',
       'mapMode=road,'
       'leftimage=images/UIUCHorizWhite.png,',
       'rightimage=images/UIUCHorizWhite.png'
       )			-- as renderAttributes
       );


-- ----------------------------------
-- -- Places
-- ----------------------------------

INSERT INTO nodes
VALUES
	(100, 'GPO', 43.38994, -71.1474, NULL, 'images/GENI.jpg'),
	(200, 'Clemson', 34.67932, -82.8352, NULL, 'images/Clemson.png'),
	(300, 'GaTech', 33.77824, -84.3989, NULL, 'images/GaTech.jpg'),
	(400, 'Indiana', 39.1699, -86.515, NULL, 'images/Indiana.jpg'),
	(500, 'Stanford', 37.42582, -122.1659, NULL, 'images/Stanford.jpg'),
	(900, 'UIUC', 40.12, -88.24, NULL, 'images/UIUCVert.png');

-- Icons for each location
INSERT into visuals
SELECT id as id,
       'pathlet' as slicename,
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
       'width=100,height=100' as renderAttributes
FROM   nodes
WHERE  name in ('GPO', 'Clemson', 'GaTech', 'Indiana', 'Stanford', 'UIUC');

-- Keep logos from overlapping
UPDATE visuals
SET    renderAttributes = 'width=100,height=100,alignment=left'
WHERE  name = 'Clemson';

UPDATE visuals
SET    renderAttributes = 'width=100,height=100,alignment=right'
WHERE  name = 'GaTech';

UPDATE visuals
SET    renderAttributes = 'width=100,height=100,alignment=bottom'
WHERE  name = 'UIUC';

-- -------------------------------------
-- - Links
-- -------------------------------------

-- Insert links
INSERT INTO links
VALUES (1000, 'fixme', 'GPO', 'Clemson'),
       (1100, 'fixme', 'Clemson', 'GaTech'),
       (1200, 'fixme', 'GaTech', 'Stanford'),
       (1300, 'fixme', 'GPO', 'Indiana'),
       (1400, 'fixme', 'Indiana', 'Stanford');

-- Fix names
UPDATE links
SET    name = CONCAT(sourceNode, '-', destNode)
WHERE  name = 'fixme';


-- Insert arcs for two pathlets
INSERT into visuals
VALUES (
       1001,			-- as id,
       'pathlet',		-- as slicename,
       'pathlet1',		-- as name,
       NULL,			-- as subSlice,
       0,			--  as sequence,
       'arc',			--  as infoType,
       'link',			--  as objType,
       'GPO-Clemson',		--  as objName,
       NULL,			--  as statType,
       NULL,			--  as statHistory,
       NULL,			--  as minValue,
       NULL,			--  as maxValue,
       NULL,			--  as statQuery,
       'pathlet1',		-- as statusHandle,
       'datapath=GPO:Clemson:GaTech:Stanford'	-- as renderAttributes
       );

INSERT into visuals
VALUES (
       1002,			-- as id,
       'pathlet',		-- as slicename,
       'pathlet2',		-- as name,
       NULL,			-- as subSlice,
       0,			--  as sequence,
       'arc',			--  as infoType,
       'link',			--  as objType,
       'GPO-Indiana',		--  as objName,
       NULL,			--  as statType,
       NULL,			--  as statHistory,
       NULL,			--  as minValue,
       NULL,			--  as maxValue,
       NULL,			--  as statQuery,
       'pathlet2',		-- as statusHandle,
       'datapath=GPO:Indiana:Stanford'	-- as renderAttributes
       );


-- Graphs for each link
INSERT into visuals
SELECT id+10 as id,
       'pathlet' as slicename,
       CONCAT(name, '-graph') as name,
       NULL as subSlice,
       1 as sequence,
       'lineGraph' as infoType,
       'link' as objType,
       name as objName,
       'Kbps' as statType,
       40 as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       CONCAT(name, '-graph') as statusHandle,
       NULL as renderAttributes
FROM   links
WHERE  id in (1200, 1400);
-- Changed following line, removing extra graphs.
-- WHERE  id in (1000, 1100, 1200, 1300, 1400);

UPDATE visuals
SET    statQuery =
       CONCAT('select timestamp as time, data_value as value ',
       	      'from GEC9_demos.GeniMeasurementDataItem ',
	      'where data_type = 1 and data_source = 7 ',
	      'order by time desc limit 40'),
       renderAttributes = 'alignment=topright'
WHERE  id = 1210;

UPDATE visuals
SET    statQuery =
       CONCAT('select timestamp as time, data_value as value ',
       	      'from GEC9_demos.GeniMeasurementDataItem ',
	      'where data_type = 1 and data_source = 6 ',
	      'order by time desc limit 40'),
       renderAttributes = 'alignment=bottomright'
WHERE  id = 1410;


-- Status items for nodes
INSERT into status
SELECT NULL as id,
       'pathlet' as sliceName,
       name as handle,
       'normal' as status
FROM   nodes;

INSERT into status
SELECT NULL as id,
       'pathlet' as sliceName,
       statusHandle as handle,
       'normal' as status
FROM   visuals
WHERE  name like 'pathlet%';
