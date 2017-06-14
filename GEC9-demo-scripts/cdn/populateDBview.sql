-- --------------------------------
-- -------- CDN -------------------
-- --------------------------------

-- Re-create empty database from prototype schema.
DROP DATABASE IF EXISTS	cdn;
CREATE DATABASE cdn;
USE cdn;

CREATE TABLE nodes LIKE	prototypeSchema.nodes;
CREATE TABLE staticVisuals LIKE prototypeSchema.visuals;
-- CREATE TABLE status LIKE prototypeSchema.status;
CREATE TABLE links LIKE prototypeSchema.links;

--
-- Map settings
--
INSERT into staticVisuals
VALUES (
       1,			-- as id,
       'cdn',			-- as sliceName,
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


-- Insert UUtah ProtoGENI site
INSERT INTO nodes
VALUES (100, 'UUtah', 40.768652, -111.82755, NULL, 'images/UUtah.jpg');

-- Insert UKy ProtoGENI site
INSERT INTO nodes
VALUES (200, 'UKentucky', 38.03, -84.49, NULL, 'images/UKentucky.jpg');

-- Insert a place for summary statistics.
INSERT INTO nodes
VALUES (300, 'AtlanticOcean', 41, -70, NULL, NULL);

-- Williams
INSERT INTO nodes
VALUES (900, 'Williams', 42.71, -73.21, NULL, 'images/WilliamsWhite.png');

-- Insert all the PlanetLab sites in nodes list.
INSERT into nodes
SELECT id+1000 as id,
       hostname as name,
       lat as latitude,
       lon as longitude,
       NULL as type,
       'images/PlanetLabCropped.png' as icon
FROM   jkarlin.cdn_host_pool
WHERE  testbed = 'planetlab';

-- Icons
INSERT into staticVisuals
SELECT id as id,
       'cdn' as sliceName,
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
       'width=100,height=100' as renderAttributes
FROM   nodes
WHERE  name in ('UUtah', 'UKentucky', 'Williams');

UPDATE staticVisuals
SET    renderAttributes='width=100,height=100,alignment=bottomright'
WHERE  name='Williams';

-- UsageGrid (Utah)
INSERT into staticVisuals
SELECT id+1 as id,
       'cdn' as sliceName,
       'UUtah-usage' as name,
       NULL as subSlice,
       2 as sequence,
       'usageGrid' as infoType,
       'node' as objType,
       name as objName,
       'ProtoGENI' as statType,
       NULL as statHistory,
       NULL as minValue,
       14 as maxValue,
       CONCAT('select now() as time, ',
       	      'count(distinct hostname) as value ',
	      'from jkarlin.cdn_hosts ',
	      'where hostname like ''%.emulab.net'' ',
	      'and hostname not like ''%uky.emulab.net'' ',
	      'limit 1') as statQuery,
       NULL as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  name = 'UUtah';

-- UsageGrid (KY)
INSERT into staticVisuals
SELECT id+1 as id,
       'cdn' as sliceName,
       'UKy-usage' as name,
       NULL as subSlice,
       2 as sequence,
       'usageGrid' as infoType,
       'node' as objType,
       name as objName,
       'ProtoGENI' as statType,
       NULL as statHistory,
       NULL as minValue,
       7 as maxValue,
       CONCAT('select now() as time, ',
       	      'count(distinct hostname) as value ',
	      'from jkarlin.cdn_hosts ',
	      'where hostname like ''%uky.emulab.net'' ',
	      'limit 1') as statQuery,
       NULL as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  name = 'UKentucky';


-- Make a visual entry for every PlanetLab node.
INSERT into staticVisuals
SELECT id+1 as id,
       'cdn' as sliceName,
       name as name,
       NULL as subSlice,
       0 as sequence,
       'point' as infoType,
       'node' as objType,
       name as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       CONCAT(name, '-status') as statusHandle,
       NULL as renderAttributes
FROM   nodes
WHERE  id > 1000;

-- Make a visual entry for experiment counter.
INSERT into staticVisuals
SELECT 2000 as id,
       'cdn' as sliceName,
       'experimentCounter' as name,
       NULL as subSlice,
       1 as sequence,
       'scalarText' as infoType,
       'node' as objType,
       'AtlanticOcean' as objName,
       'Experiments' as statType,
       0 as statHistory,
       NULL as minValue,
       NULL as maxValue,
       'select now() as time, count(distinct experiment) as value from jkarlin.cdn_hosts limit 1;' as statQuery,
       'expCounter' as statusHandle,
       NULL as renderAttributes;


--
-- Dynamic visuals for experiment IDs.
--
CREATE VIEW dynamicVisuals AS
SELECT DISTINCT
       NULL as id,
       'cdn' as sliceName,
       CONCAT(experiment, '-label') as name,
       NULL as subSlice,
       2 as sequence,
       'label' as infoType,
       'node' as objType,
       'AtlanticOcean' as objName,
       NULL as statType,
       NULL as statHistory,
       NULL as minValue,
       NULL as maxValue,
       NULL as statQuery,
       CONCAT(experiment, '-label') as statusHandle,
       CONCAT('text=', experiment) as renderAttributes
FROM   jkarlin.cdn_hosts;



CREATE VIEW visuals AS
SELECT * from staticVisuals
UNION
SELECT * from dynamicVisuals
ORDER BY NAME;


-- Setup status as a view.  Everything has status 'normal',
-- unless it's a planetlab host listed in the host_pool, but
-- not in the hosts table, which means it's not currently
-- active and should be status 'hidden'

CREATE VIEW status AS
SELECT NULL as id,
       'cdn' as sliceName,
       CONCAT(name, '-status') as handle,
       IF ((name in (select hostname from jkarlin.cdn_host_pool)) AND
       	   (name not in (select hostname from jkarlin.cdn_hosts)),
	   'normal',
	   'active') as status
FROM visuals;
