<?php 
    include('general.php');

    //Database connection
    $db = General::Connect();

    // Strings must be escaped to prevent SQL injection attack. 
    $p_username = mysql_real_escape_string($_GET["username"], $db); 
    $p_password = mysql_real_escape_string($_GET["password"], $db); 

    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = md5($p_username . $p_password . General::SecretKey()); //Create our own hash

    if($real_hash == $p_hash)
    {
        //Check if username & password are correct (anti SQL-injection)
        $query = sprintf("SELECT * FROM cyvasse_users WHERE username='%s'", mysql_real_escape_string($p_username));
        $result = mysql_query($query);
        $row = mysql_fetch_assoc($result);
        $rowcount = mysql_num_rows($result);

        //Do we even have a record with this username?       
        if ($rowcount < 1)
        {
            //Nope? Then this username simply doesn't exist
            General::ReturnSuccess(false);
            echo("Incorrect username.");
            return;
        }

        //Check for the correct password
        if ($p_password != $row["password"])
        {
            General::ReturnSuccess(false);
            echo("Incorrect password.");
            return;
        }

        //Update last login time
        $currentDate = date('Y-m-d H:i:s');
        $id = $row['user_id'];
        mysql_query("UPDATE cyvasse_users SET last_login_date = '$currentDate' WHERE user_id = '$id'") or die (mysql_error());

        //Everything succeeded?
        General::ReturnSuccess(true);
        echo($id);
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match in login!");
    }
?>