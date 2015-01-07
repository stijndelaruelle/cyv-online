<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    // Strings must be escaped to prevent SQL injection attack. 
    $p_userID   = mysql_real_escape_string($_GET["p"], $db);
    $p_gameID   = mysql_real_escape_string($_GET["g"], $db);

    for ($i = 0; $i < 26; ++$i)
    {
        $p_unitID[$i] = mysql_real_escape_string($_GET["u" . $i], $db); 
    }   

    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = $p_userID . $p_gameID; //Create our own hash

    for ($i = 0; $i < 26; ++$i)
    {
        $real_hash .= $p_unitID[$i]; 
    }   

    $real_hash .= General::SecretKey(); //Create our own hash

    $real_hash = md5($real_hash);
    if($real_hash == $p_hash)
    {
        //Get the board ID
        $query = sprintf("SELECT * FROM cyvasse_games WHERE game_id = '$p_gameID'");
        $result = mysql_query($query) or die (mysql_error());
        $row = mysql_fetch_assoc($result);
        $boardID = $row['board_id'];

        if ($row['game_state'] != 0)
        {
            General::ReturnSuccess(false);
            echo("The game is is not in setup mode, how are you even calling this script?");
            return;
        }

        //Alter the board
        $offset = 0;
        if ($p_userID == $row['user_id_black']) $offset = 26;

        //build the query
        $query = sprintf("UPDATE cyvasse_boards ");
        for ($i = 0; $i < 26; ++$i)
        {
            $fieldID = "unit_" . ($i + $offset);
            $unitID = $p_unitID[$i];

            if ($i == 0) $query .= "SET ";
            else         $query .= ", ";

            $query .= sprintf("$fieldID = '$unitID' ");
        }   

        $query .= sprintf("WHERE board_id = '$boardID'");

        //Execute the query
        mysql_query($query) or die (mysql_error());


        //Check if both players submitted, if so the game moves on to the next state (white player move)

        //mysql_query("UPDATE cyvasse_games SET game_state = '1' WHERE game_id = '$p_gameID'") or die (mysql_error());

        //Update 
        $white_id = $row['user_id_white'];
        $black_id = $row['user_id_black'];
        mysql_query("UPDATE cyvasse_users SET refresh = '1' WHERE user_id = '$white_id'") or die (mysql_error());
        mysql_query("UPDATE cyvasse_users SET refresh = '1' WHERE user_id = '$black_id'") or die (mysql_error());

        General::ReturnSuccess(true);
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match in setup units!");
    }
?>