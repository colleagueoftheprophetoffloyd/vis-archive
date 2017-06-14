-- --------------------------------
-- ------ Violin ------------------
-- --------------------------------

-- Re-create empty database from prototype schema.
DROP DATABASE IF EXISTS violin;
CREATE DATABASE violin;
USE violin;

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
       'violin',		-- as slicename,
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
       'leftimage=images/DocomoWhite.png,',
       'rightimage=images/Purdue.png'
       )			-- as renderAttributes
       );

-- Insert UUtah ProtoGENI site
INSERT INTO nodes
VALUES (100, 'Utah', 40.768652, -111.82755, NULL, 'images/UUtah.jpg');

-- Insert GPO ProtoGENI site
INSERT INTO nodes
VALUES (200, 'GPO', 42.38994, -71.1474, NULL, 'images/GENI.jpg');

-- And a link between them
INSERT INTO links
VALUES (1000, 'Utah-GPO', 'Utah', 'GPO');

-- Make two visual entries for each ProtoGENI site.
-- Icon
INSERT into visuals
SELECT id as id,
       'violin' as slicename,
       nodes.name as name,
       NULL as subSlice,
       1 as sequence,
       'zoomButton' as infoType,
       'node' as objType,
       name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       nodes.name as statusHandle,
       CONCAT(
       'zoomTarget=', name,
       ',mapwidth=500,mapheight=300'
       ) as renderAttributes
FROM   nodes
WHERE  name in ('Utah', 'GPO');

-- UsageGrids for ProtoGENI sites
INSERT into visuals
SELECT id+1 as id,
       'violin' as slicename,
       CONCAT(nodes.name, '-usage') as name,
       NULL as subSlice,
       2 as sequence,
       'usageGrid' as infoType,
       'node' as objType,
       name as objName,
       'ProtoGENI' as statType,
       NULL as statHistory,
       NULL as minValue,
       30 as maxValue,
       CONCAT(
       'select time, value ',
       'from statistics ',
       'where stat=''', name, '-PGUsage'' ',
       'order by time desc limit 1') as statQuery,
       name as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  name in ('Utah', 'GPO');


-- Arc for link
INSERT into visuals
SELECT id as id,
       'violin' as slicename,
       links.name as name,
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
       links.name as statusHandle,
       CONCAT('datapath=', links.sourceNode, ':', links.destNode) as renderAttributes
FROM   links;

-- Line graph for link
INSERT into visuals
SELECT id+1 as id,
       'violin' as slicename,
       CONCAT(links.name, '-graph') as name,
       NULL as subSlice,
       2 as sequence,
       'lineGraph' as infoType,
       'link' as objType,
       name as objName,
       'MB' as statType,
       40 as statHistory,
       NULL as minValue,
       NULL as maxValue,
       CONCAT(
       'select time, value ',
       'from statistics ',
       'where stat=''', links.name, '-BWUsage'' ',
       'order by time desc limit 40') as statQuery,
       NULL as statusHandle,
       'alignment=top' as renderAttributes
FROM   links;

-- ------------------ END MAIN SECTION ---------------





-- ------------------- Utah SECTION ------------------
--
-- Utah detail
--

INSERT INTO nodes
VALUES (10100, 'Utah-switch', 0, -100, NULL, NULL),
       (10200, 'Utah-controller', 1, -100, NULL, NULL),
       (10300, 'Utah-A', 0, 0, NULL, NULL),
       (10400, 'Utah-B', 0, 0, NULL, NULL),
       (10500, 'Utah-C', 0, 0, NULL, NULL),
       (10600, 'Utah-D', 0, 0, NULL, NULL),
       (10700, 'Utah-E', 0, 0, NULL, NULL),
       (10800, 'Utah-F', 0, 0, NULL, NULL);

UPDATE nodes
SET    latitude = sin(3.14/3.0*(id-10550)/100.0/3.0),
       longitude = -(100+cos(3.14/3.0*(id-10550)/100.0/3.0))
WHERE  id in (10300, 10400,
       	      10500, 10600, 10700, 10800);

UPDATE nodes
SET    icon = 'images/OpenFlowSmall.png'
WHERE  name = 'Utah-switch';

--
-- Utah inset map settings
--
INSERT into visuals
VALUES (
       10001,			-- as id,
       'violin',		-- as slicename,
       'UtahMap',		-- as name,
       'Utah',			-- as subSlice,
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
       'centerLat=0.0,',
       'centerLon=-100.5,',
       'zoomLevel=7,',
       'mapMode=black'
       )			-- as renderAttributes
       );


INSERT INTO visuals
SELECT id as id,
       'violin' as slicename,
       nodes.name as name,
       'Utah' as subSlice,
       1 as sequence,
       'label' as infoType,
       'node' as objType,
       nodes.name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       nodes.name as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  id in (10100, 10200);

UPDATE visuals
SET    infoType = 'icon',
       renderAttributes='alignment=left'
WHERE  objName = 'Utah-switch';

UPDATE visuals
SET    renderAttributes='alignment=bottom'
WHERE  objName = 'Utah-controller';

INSERT INTO visuals
SELECT id+1 as id,
       'violin' as slicename,
       nodes.name as name,
       'Utah' as subSlice,
       2 as sequence,
       'scalar' as infoType,
       'node' as objType,
       nodes.name as objName,
       nodes.name as statType,
       NULL as statHistory,
       0 as minValue,
       1 as maxValue,
       CONCAT(
       'select time, value ',
       'from statistics ',
       'where stat=''', nodes.name, '-load'' ',
       'order by time desc limit 1') as statQuery,
       nodes.name as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  id in (10300, 10400,
       	      10500, 10600, 10700, 10800);

UPDATE visuals
SET    renderAttributes='alignment=right'
WHERE  objName in ('Utah-A', 'Utah-B', 'Utah-C',
       	       	   'Utah-D', 'Utah-E', 'Utah-F');

-- Add some links

INSERT into links
SELECT id as id,
       CONCAT(name, '-', 'Utah-switch') as name,
       name as sourceNode,
       'Utah-switch' as destNode
FROM   nodes
WHERE  id in (10200, 10300, 10400,
       	      10500, 10600, 10700, 10800);


-- Arcs for links
INSERT into visuals
SELECT id+10 as id,
       'violin' as slicename,
       links.name as name,
       'Utah' as subSlice,
       1 as sequence,
       'arc' as infoType,
       'link' as objType,
       links.name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       links.name as statusHandle,
       CONCAT('datapath=', links.sourceNode, ':', links.destNode) as renderAttributes
FROM   links
WHERE  id in (10200, 10300, 10400,
       	      10500, 10600, 10700, 10800);


-- ------------------ END Utah SECTION  --------------



-- ------------------- GPO SECTION ------------------
--
-- GPO detail
--

INSERT INTO nodes
VALUES (20100, 'GPO-switch', 0,100, NULL, NULL),
       (20200, 'GPO-controller', 1, 100, NULL, NULL),
       (20300, 'GPO-A', 0, 0, NULL, NULL),
       (20400, 'GPO-B', 0, 0, NULL, NULL),
       (20500, 'GPO-C', 0, 0, NULL, NULL),
       (20600, 'GPO-D', 0, 0, NULL, NULL),
       (20700, 'GPO-E', 0, 0, NULL, NULL),
       (20800, 'GPO-F', 0, 0, NULL, NULL);

UPDATE nodes
SET    latitude = sin(3.14/3.0*(id-20550)/100.0/3.0),
       longitude = 100+cos(3.14/3.0*(id-20550)/100.0/3.0)
WHERE  id in (20300, 20400,
       	      20500, 20600, 20700, 20800);

UPDATE nodes
SET    icon = 'images/OpenFlowSmall.png'
WHERE  name = 'GPO-switch';

--
-- GPO inset map settings
--
INSERT into visuals
VALUES (
       20001,			-- as id,
       'violin',		-- as slicename,
       'GPOMap',		-- as name,
       'GPO',			-- as subSlice,
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
       'centerLat=0.0,',
       'centerLon=100.5,',
       'zoomLevel=7,',
       'mapMode=black'
       )			-- as renderAttributes
       );


INSERT INTO visuals
SELECT id as id,
       'violin' as slicename,
       nodes.name as name,
       'GPO' as subSlice,
       1 as sequence,
       'label' as infoType,
       'node' as objType,
       nodes.name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       nodes.name as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  id in (20100, 20200);

UPDATE visuals
SET    infoType = 'icon',
       renderAttributes='alignment=right'
WHERE  objName = 'GPO-switch';

UPDATE visuals
SET    renderAttributes='alignment=bottom'
WHERE  objName = 'GPO-controller';

INSERT INTO visuals
SELECT id+1 as id,
       'violin' as slicename,
       nodes.name as name,
       'GPO' as subSlice,
       2 as sequence,
       'scalar' as infoType,
       'node' as objType,
       nodes.name as objName,
       nodes.name as statType,
       NULL as statHistory,
       0 as minValue,
       1 as maxValue,
       CONCAT(
       'select time, value ',
       'from statistics ',
       'where stat=''', nodes.name, '-load'' ',
       'order by time desc limit 1') as statQuery,
       nodes.name as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  id in (20300, 20400,
       	      20500, 20600, 20700, 20800);

UPDATE visuals
SET    renderAttributes='alignment=left'
WHERE  objName in ('GPO-A', 'GPO-B', 'GPO-C',
       	       	   'GPO-D', 'GPO-E', 'GPO-F');

-- Add some links

INSERT into links
SELECT id as id,
       CONCAT(name, '-', 'GPO-switch') as name,
       name as sourceNode,
       'GPO-switch' as destNode
FROM   nodes
WHERE  id in (20200, 20300, 20400,
       	      20500, 20600, 20700, 20800);


-- Arcs for links
INSERT into visuals
SELECT id+10 as id,
       'violin' as slicename,
       links.name as name,
       'GPO' as subSlice,
       1 as sequence,
       'arc' as infoType,
       'link' as objType,
       links.name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       links.name as statusHandle,
       CONCAT('datapath=', links.sourceNode, ':', links.destNode) as renderAttributes
FROM   links
WHERE  id in (20200, 20300, 20400,
       	      20500, 20600, 20700, 20800);


-- ------------------ END GPO SECTION  --------------



-- Status items for everything
INSERT into status
SELECT NULL as id,
       'violin' as sliceName,
       name as handle,
       'normal' as status
FROM   nodes;

INSERT into status
SELECT NULL as id,
       'violin' as sliceName,
       name as handle,
       'forward' as status
FROM   links;

