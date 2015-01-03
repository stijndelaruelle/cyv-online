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
        //Check if this username already exists
        if (General::GetPlayerID($p_username) != -1)
        {
            General::ReturnSuccess(false);
            echo("Username already taken!");
            return;
        }

        // Send variables for the MySQL database class. 
        $currentDate = date('Y-m-d H:i:s');

        $result = mysql_query("INSERT INTO cyvasse_users (user_id, username, password, email, ip, last_login_date, reg_date, refresh)
                                      VALUES('', '$p_username', '$p_password', 'hello@blargal.com', '', '$currentDate', '$currentDate', '1')")
                                      or die("Query failed: " . mysql_error());

        if (!$result) General::ReturnSuccess(false);
        else          General::ReturnSuccess(true);
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match in create account!");
    }
?>