<?php
include "sql_vars.php";
include "sql_vars_status.php";

$query = "SELECT `$mysql_handleCol`, `$mysql_statusCol` FROM `$mysql_statusTable` WHERE `$mysql_sliceNameCol` = '$slice';";

// execute the query and gather the results
mysql_select_db($db, $connection);
$result = mysql_query($query);
$statusArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $statusArray[] = array(
	$mysql_handleCol => $itemRow[$mysql_handleCol],
	$mysql_statusCol => $itemRow[$mysql_statusCol]
	);
}
mysql_close($connection);

// encode the results as JSON Text
// we're using a returnType field so that our Silverlight application can differentiate between
// the kind of return values it recieves and parse the Json object appropriately

$returnItems = array(
	"returnType" => "status",
	"query" => $query,
	"results" => $statusArray );
$JSONResult = json_encode($returnItems);

// and print the results so that our app can read them
echo $JSONResult;
?>


