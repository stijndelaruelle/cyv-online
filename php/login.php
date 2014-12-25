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
        //Check if username & password are correct (anti SQL-injection)
        $query = sprintf("SELECT * FROM cyvasse_users WHERE username='%s'", mysql_real_escape_string($p_username));
        $row = mysql_fetch_assoc(mysql_query($query));
        $rowcount = mysql_num_rows(mysql_query($query));

        //Do we even have a record with this username?       
        if ($rowcount < 1)
        {
            //Nope? Then this username simply doesn't exist
            echo("Incorrect username.");
            return;
        }

        //Check for the correct password
        if ($p_password != $row["password"])
        {
            echo("Incorrect password.");
            return;
        }

        //Everything succeeded?
        echo(true);
    }
    else
    {
        echo("Hashes didn't match!");
    }
?>