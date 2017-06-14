-- --------------------------------
-- -------- Create prototype schema
-- -------- MEB wrote for GEC12
-- --------------------------------

DROP DATABASE IF EXISTS prototypeSchema;
CREATE DATABASE prototypeSchema;
USE prototypeSchema;

source locations.sql;
source prototypeSchemaDump.sql
