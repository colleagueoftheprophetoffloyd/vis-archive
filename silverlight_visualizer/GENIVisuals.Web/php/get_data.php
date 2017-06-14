<?php
include "sql_vars_post.php";
include "sql_vars_data.php";

$query = $_POST['statQuery'];

// execute the query and gather the results
mysql_select_db($db, $connection);
$result = mysql_query($query);
$dataArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $dataArray[] = array(
	$mysql_timeCol => $itemRow[$mysql_timeCol],
	$mysql_valueCol => $itemRow[$mysql_valueCol]
	);
}
mysql_close($connection);

// encode the results as JSON Text
// we're using a returnType field so that our Silverlight application can differentiate between
// the kind of return values it recieves and parse the Json object appropriately

$returnItems = array(
	"returnType" => "data",
	"query" => $query,
	"results" => $dataArray );
$JSONResult = json_encode($returnItems);

// and print the results so that our app can read them
echo $JSONResult;
?>


