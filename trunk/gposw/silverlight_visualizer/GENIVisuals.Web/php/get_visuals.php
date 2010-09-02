<?php
include "sql_vars.php";
include "sql_vars_visuals.php";

$query = "SELECT * FROM `$mysql_visualsTable` WHERE `$mysql_sliceNameCol` = '$slice' order by `$mysql_sequenceCol`;";

// execute the query and gather the results
mysql_select_db($db, $connection);
$result = mysql_query($query);
$visualArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $visualArray[] = array(
	$mysql_IDCol => $itemRow[$mysql_IDCol],
	$mysql_sliceNameCol => $itemRow[$mysql_sliceNameCol],	
	$mysql_sequenceCol => $itemRow[$mysql_sequenceCol],
	$mysql_infoTypeCol => $itemRow[$mysql_infoTypeCol],
	$mysql_objTypeCol => $itemRow[$mysql_objTypeCol],
	$mysql_objNameCol => $itemRow[$mysql_objNameCol],
	$mysql_statTypeCol => $itemRow[$mysql_statTypeCol],
	$mysql_statHistoryCol => $itemRow[$mysql_statHistoryCol],
	$mysql_minValueCol => $itemRow[$mysql_minValueCol],
	$mysql_maxValueCol => $itemRow[$mysql_maxValueCol],
	$mysql_statQueryCol => $itemRow[$mysql_statQueryCol],
  $mysql_statusHandleCol => $itemRow[$mysql_statusHandleCol],
  $mysql_renderAdviceCol => $itemRow[$mysql_renderAdviceCol],
  $mysql_positionAdviceCol => $itemRow[$mysql_positionAdviceCol]
	);
}
mysql_close($connection);

// encode the results as JSON Text
// we're using a returnType field so that our Silverlight application can differentiate between
// the kind of return values it recieves and parse the Json object appropriately

$returnItems = array(
	"returnType" => "visuals",
	"query" => $query,
	"results" => $visualArray );
$JSONResult = json_encode($returnItems);

// and print the results so that our app can read them
echo $JSONResult;
?>


