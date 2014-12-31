<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    // Strings must be escaped to prevent SQL injection attack. 
    $p_username = mysql_real_escape_string($_GET["username"], $db); 


    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = md5($p_username . General::SecretKey()); //Create our own hash

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
            General::ReturnSuccess(false);
            echo("User not found.");
            return;
        }

        //Return some data about the user
        General::ReturnSuccess(true);
        echo($row['username']);
        //echo("avatar");
        //...
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match!");
    }
?>