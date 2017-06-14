<?php
if (array_key_exists('slice', $_POST)) {
    $slice = $_POST['slice'];
  } else {
    $slice = 'TestSlice';
  }
if (array_key_exists('server', $_POST)) {
    $server = $_POST['server'];
  } else {
    $server = "localhost";
  }
if (array_key_exists('dbUsername', $_POST)) {
    $dbUsername = $_POST['dbUsername'];
  } else {
    $dbUsername = "mberman";
  }
if (array_key_exists('dbPassword', $_POST)) {
    $dbPassword = $_POST['dbPassword'];
  } else {
    $dbPassword = "";
  }
if (array_key_exists('db', $_POST)) {
    $db = $_POST['db'];
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

