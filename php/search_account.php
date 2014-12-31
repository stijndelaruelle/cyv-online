<?php
    //Database connection
    $db = mysql_connect("localhost", "blargal_main", "wafels") or die("Could not connect to database: " . mysql_error()); 
    mysql_select_db("blargal_main") or die("Could not select database: blargal_main");
 
     // Strings must be escaped to prevent SQL injection attack. 
    $p_keyword = mysql_real_escape_string($_GET["keyword"], $db); 

    //Hash generated from the application
    $p_hash = $_GET["hash"];
    $secretKey = "CyvasseKey"; // Change this value to match the value stored in the client javascript below 

    //Create our own hash & compare it
    $real_hash = md5($p_keyword . $secretKey);

    if($real_hash == $p_hash)
    {
        //Check if username & password are correct (anti SQL-injection)
        $query = sprintf("SELECT * FROM cyvasse_users WHERE username='%s'", mysql_real_escape_string($p_username));
        $row = mysql_fetch_assoc(mysql_query($query));
        $rowcount = mysql_num_rows(mysql_query($query));

        //Do we even have a record with this username?       
        if ($rowcount < 1)
        {
            //Nope? Then this username simply doesn't exist
            echo(false);
            echo("User not found.");
            return;
        }

        //Return some data about the user
        echo(true);
        echo(p_username);
        //echo("avatar");
        //...
    }
    else
    {
        echo("Hashes didn't match!");
    }
?>