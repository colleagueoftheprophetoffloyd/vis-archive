-- MySQL dump 10.13  Distrib 5.1.47, for redhat-linux-gnu (x86_64)
--
-- Host: localhost    Database: mebtest2
-- ------------------------------------------------------
-- Server version	5.1.47

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `locations`
--

DROP TABLE IF EXISTS `locations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `locations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `category` varchar(200) DEFAULT NULL,
  `cityName` varchar(200) DEFAULT NULL,
  `latitude` double DEFAULT NULL,
  `longitude` double DEFAULT NULL,
  `description` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=35 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `locations`
--

LOCK TABLES `locations` WRITE;
/*!40000 ALTER TABLE `locations` DISABLE KEYS */;
INSERT INTO `locations` VALUES (1,'OpenFlow','Stanford ',37.42582,-122.1659,''),(2,'OpenFlow','Clemson ',34.67932,-82.8352,''),(3,'OpenFlow','Georgia Tech ',33.77824,-84.3989,''),(4,'OpenFlow','Indiana University ',39.1699,-86.515,''),(5,'OpenFlow','Princeton ',40.3446,-74.65477,''),(6,'OpenFlow','Washington University ',38.6487,-90.3073,''),(7,'OpenFlow','Wisconsin University at Madison ',43.0762,-89.4123,''),(8,'OpenFlow','BBN ',42.38994,-71.1474,''),(9,'NLR','Sunnyvale California ',37.36199,-122.04708,''),(10,'NLR','Seattle Washington ',47.61441,-122.33832,''),(11,'NLR','Denver Colorado ',39.80792,-105.1159,''),(12,'NLR','Chicago Illionois ',41.883507,-87.639608,''),(13,'NLR','Atlanta Georgia ',33.76427,-84.38363,''),(14,'I2','Salt Lake City, Utah ',40.75203,-111.82755,'* This is a \"placeholder city.\"\"  None of the I2 mesoscale resources have yet been deployed.  Although we\'re pretty sure that the 4 other ciites will be the actual cities used in deployment'),(15,'I2','New York, NY ',0,0,''),(16,'I2','Houston, Texas ',29.76303,-95.383099,''),(17,'I2','Chicago, IL',41.89719,-87.62212,''),(18,'I2','Atlanta Georgia ',33.77821,-84.39785,''),(19,'WiMax','BBN ',42.38994,-71.1474,'The first three are up and running. The rest may come online before GEC9'),(20,'WiMax','Rutgers ',40.74187,-74.17575,''),(21,'WiMax','Stanford ',37.42582,-122.1659,''),(22,'WiMax','Columbia University ',40.8093,-73.96018,''),(23,'WiMax','University of Colorado Boulder',40.00902,-105.26973,''),(24,'WiMax','Polytechnic Institute of New York University',40.69286,-73.984607,''),(25,'WiMax','UCLA ',34.41821,-119.85248,''),(26,'WiMax','UMass Amherst ',42.3929,-72.5228,''),(27,'WiMax','University of Wisconsin Madison ',43.0762,-89.4123,''),(28,'ShadowNet\n','Salt Lake City, UT ',40.75203,-111.82755,''),(29,'ShadowNet\nShadowNet\n','Washington D.C. ',38.996308,-76.9298,''),(30,'ShadowNet\n','Kansas City, MO ',39.07656,-94.55518,''),(31,'ShadowNet\n','Houston, TX ',29.76303,-95.383099,''),(32,'ProtoGENI','Salt Lake City, UT ',40.75203,-111.82755,'Might be two more in place by GEC9, but I don\'t know yet, so lets just go with these three. '),(33,'ProtoGENI','Washington D.C. ',38.996308,-76.9298,''),(34,'ProtoGENI','Kansas City, MO ',39.07656,-94.55518,'');
/*!40000 ALTER TABLE `locations` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2010-09-22 14:53:29
