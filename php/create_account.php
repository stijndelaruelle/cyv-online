<?php 
    //Database connection
    $db = mysql_connect("localhost", "blargal_main", "wafels") or die("Could not connect to database: " . mysql_error()); 
    mysql_select_db("blargal_main") or die("Could not select database: blargal_main");

    // Strings must be escaped to prevent SQL injection attack. 
    $p_username = mysql_real_escape_string($_GET["username"], $db); 
    $p_password = mysql_real_escape_string($_GET["password"], $db); 

    //Hash generated from the application
    $hash = $_GET["hash"];
    $secretKey = "CyvasseKey"; // Change this value to match the value stored in the client javascript below 

    //Create our own hash & compare it
    $real_hash = md5($p_username . $p_password . $secretKey);

    if($real_hash == $hash)
    {
        // Send variables for the MySQL database class. 
        $result = mysql_query("INSERT INTO cyvasse_users (user_id, username, password) VALUES('', '$p_username', '$p_password')") or die("Query failed: " . mysql_error()); 
    }
    else
    {
        echo("Hashes didn't match!");
    }
?>