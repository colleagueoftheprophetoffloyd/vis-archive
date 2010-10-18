<?php
include "sql_vars_post.php";

// Vars for nodes table.
$mysql_nodesTable  = "nodes";
$mysql_nodesIDCol = "id";
$mysql_nodesNameCol = "name";
$mysql_nodesLatitudeCol = "latitude";
$mysql_nodesLongitudeCol = "longitude";
$mysql_nodesTypeCol = "type";
$mysql_nodesIconCol = "icon";

// Vars for links table.
$mysql_linksTable  = "links";
$mysql_linksIDCol = "id";
$mysql_linksNameCol = "name";
$mysql_linksSourceNodeCol = "sourceNode";
$mysql_linksDestNodeCol = "destNode";

// Vars for visuals table.
$mysql_visualsTable  = "visuals";
$mysql_visualsIDCol = "id";
$mysql_visualsSliceNameCol = "sliceName";
$mysql_visualsNameCol = "name";
$mysql_visualsSubSliceCol = "subSlice";
$mysql_visualsSequenceCol = "sequence";
$mysql_visualsInfoTypeCol = "infoType";
$mysql_visualsObjTypeCol = "objType";
$mysql_visualsObjNameCol = "objName";
$mysql_visualsStatTypeCol = "statType";
$mysql_visualsStatHistoryCol = "statHistory";
$mysql_visualsMinValueCol = "minValue";
$mysql_visualsMaxValueCol = "maxValue";
$mysql_visualsStatQueryCol = "statQuery";
$mysql_visualsStatusHandleCol = "statusHandle";
$mysql_visualsRenderAttributesCol = "renderAttributes";

// Vars for status table.
$mysql_statusTable = "status";
$mysql_statusSliceNameCol = "sliceName";
$mysql_statusHandleCol = "handle";
$mysql_statusStatusCol = "status";


// Connect to database.
mysql_select_db($db, $connection);


// Built this the ugly way.  Run each
// query in sequence.

// ***************** NODES Section

$query = "SELECT * FROM `$mysql_nodesTable` ;";

// execute the query and gather the results
$result = mysql_query($query);
$nodeArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $nodeArray[] = array(
	$mysql_nodesIDCol => $itemRow[$mysql_nodesIDCol],
	$mysql_nodesNameCol => $itemRow[$mysql_nodesNameCol],
	$mysql_nodesLatitudeCol => $itemRow[$mysql_nodesLatitudeCol],
	$mysql_nodesLongitudeCol => $itemRow[$mysql_nodesLongitudeCol],
	$mysql_nodesTypeCol => $itemRow[$mysql_nodesTypeCol],
	$mysql_nodesIconCol => $itemRow[$mysql_nodesIconCol]
	);
}
$nodeReturnItems = array(
	"returnType" => "nodes",
	"query" => $query,
	"results" => $nodeArray );

// ******************* Links section
 
$query = "SELECT * FROM `$mysql_linksTable` ;";

$result = mysql_query($query);
$linkArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $linkArray[] = array(
	$mysql_linksIDCol => $itemRow[$mysql_linksIDCol],
	$mysql_linksNameCol => $itemRow[$mysql_linksNameCol],
	$mysql_linksSourceNodeCol => $itemRow[$mysql_linksSourceNodeCol],
	$mysql_linksDestNodeCol => $itemRow[$mysql_linksDestNodeCol],
	);
}

$linkReturnItems = array(
	"returnType" => "links",
	"query" => $query,
	"results" => $linkArray );


// ********************* Visuals section

$query = "SELECT * FROM `$mysql_visualsTable` WHERE `$mysql_visualsSliceNameCol` = '$slice' order by `$mysql_visualsSequenceCol`;";

$result = mysql_query($query);
$visualArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $visualArray[] = array(
	$mysql_visualsIDCol => $itemRow[$mysql_visualsIDCol],
	$mysql_visualsSliceNameCol => $itemRow[$mysql_visualsSliceNameCol],	
	$mysql_visualsNameCol => $itemRow[$mysql_visualsNameCol],	
	$mysql_visualsSubSliceCol => $itemRow[$mysql_visualsSubSliceCol],	
	$mysql_visualsSequenceCol => $itemRow[$mysql_visualsSequenceCol],
	$mysql_visualsInfoTypeCol => $itemRow[$mysql_visualsInfoTypeCol],
	$mysql_visualsObjTypeCol => $itemRow[$mysql_visualsObjTypeCol],
	$mysql_visualsObjNameCol => $itemRow[$mysql_visualsObjNameCol],
	$mysql_visualsStatTypeCol => $itemRow[$mysql_visualsStatTypeCol],
	$mysql_visualsStatHistoryCol => $itemRow[$mysql_visualsStatHistoryCol],
	$mysql_visualsMinValueCol => $itemRow[$mysql_visualsMinValueCol],
	$mysql_visualsMaxValueCol => $itemRow[$mysql_visualsMaxValueCol],
	$mysql_visualsStatQueryCol => $itemRow[$mysql_visualsStatQueryCol],
	$mysql_visualsStatusHandleCol => $itemRow[$mysql_visualsStatusHandleCol],
	$mysql_visualsRenderAttributesCol => $itemRow[$mysql_visualsRenderAttributesCol]
	);
}

$visualReturnItems = array(
	"returnType" => "visuals",
	"query" => $query,
	"results" => $visualArray );

// ********************* Status section

$query = "SELECT `$mysql_statusHandleCol`, `$mysql_statusStatusCol` FROM `$mysql_statusTable` WHERE `$mysql_statusSliceNameCol` = '$slice';";

$result = mysql_query($query);
$statusArray = array();

while($itemRow = mysql_fetch_array($result))
{
  $statusArray[] = array(
	$mysql_statusHandleCol => $itemRow[$mysql_statusHandleCol],
	$mysql_statusStatusCol => $itemRow[$mysql_statusStatusCol]
	);
}

$statusReturnItems = array(
	"returnType" => "status",
	"query" => $query,
	"results" => $statusArray );


// *******************************
mysql_close($connection);

// Gather all query results into one big mess

$allResults = array();
$allResults[] = $nodeReturnItems;
$allResults[] = $linkReturnItems;
$allResults[] = $visualReturnItems;
$allResults[] = $statusReturnItems;
$wrappedResults = array("returnType" => "mixed",
			"results" => $allResults);
$JSONResult = json_encode($wrappedResults);

// And print the result so our app can read it.
echo $JSONResult;
?>


