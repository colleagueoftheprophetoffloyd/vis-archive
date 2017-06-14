<?php
if (array_key_exists('slice', $_GET)) {
    $slice = $_GET['slice'];
  } else {
    $slice = 'TestSlice';
  }
if (array_key_exists('server', $_GET)) {
    $server = $_GET['server'];
  } else {
    $server = "localhost";
  }
if (array_key_exists('dbUsername', $_GET)) {
    $dbUsername = $_GET['dbUsername'];
  } else {
    $dbUsername = "mberman";
  }
if (array_key_exists('dbPassword', $_GET)) {
    $dbPassword = $_GET['dbPassword'];
  } else {
    $dbPassword = "";
  }
if (array_key_exists('db', $_GET)) {
    $db = $_GET['db'];
  } else {
    $db = "mebtest";
  }

$connection = mysql_connect($server, $dbUsername, $dbPassword);

function formatInput($rawURLData)
{
$returnString = urldecode($rawURLData);
$returnString = mysql_real_escape_string($returnString);
return $returnString;
}
?>

