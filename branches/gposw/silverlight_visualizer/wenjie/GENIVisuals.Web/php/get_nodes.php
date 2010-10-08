<?php
include "sql_vars.php";
include "sql_vars_nodes.php";

// Commented out example of building where clause from passed arg
// in ?itemStatus="done" of URL

//    set up the “itemStatus” URL option and build a query addition
//        to account for the itemStatus variable
//$itemStatus = $_GET['itemStatus'];
//$itemQueryAddition = “”;
//
//if($itemStatus != NULL)
//{
//if($itemStatus == “done”)
//{
//$itemQueryAddition = “WHERE `$mysql_isDoneCol` = 1″;
//} else if ($itemStatus == “notDone”)
//{
//$itemQueryAddition = “WHERE `$mysql_isDoneCol` = 0″;
//}

// Construct our MySQL query
//$todoQuery = “SELECT * FROM `$mysql_todoTable` $itemQueryAddition ;”;
 
$query = "SELECT * FROM `$mysql_nodesTable` ;";

// execute the query and gather the results
mysql_select_db($db, $connection);
$result = mysql_query($query);
$nodeArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $nodeArray[] = array(
	$mysql_IDCol => $itemRow[$mysql_IDCol],
	$mysql_nameCol => $itemRow[$mysql_nameCol],
	$mysql_latitudeCol => $itemRow[$mysql_latitudeCol],
	$mysql_longitudeCol => $itemRow[$mysql_longitudeCol],
	$mysql_typeCol => $itemRow[$mysql_typeCol],
	$mysql_iconCol => $itemRow[$mysql_iconCol]
	);
}
mysql_close($connection);

// encode the results as JSON Text
// we're using a returnType field so that our Silverlight application can differentiate between
// the kind of return values it recieves and parse the Json object appropriately

$returnItems = array(
	"returnType" => "nodes",
	"query" => $query,
	"results" => $nodeArray );
$JSONResult = json_encode($returnItems);

// and print the results so that our app can read them
echo $JSONResult;
?>


