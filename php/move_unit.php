<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    // Strings must be escaped to prevent SQL injection attack. 
    $p_userID   = mysql_real_escape_string($_GET["p"], $db);
    $p_gameID   = mysql_real_escape_string($_GET["g"], $db);
    $p_unitID   = mysql_real_escape_string($_GET["u"], $db); 
    $p_tileID   = mysql_real_escape_string($_GET["t"], $db); 
    $p_killedID = mysql_real_escape_string($_GET["k"], $db); 

    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = md5($p_userID . $p_gameID . $p_unitID . $p_tileID . $p_killedID . General::SecretKey()); //Create our own hash

    if($real_hash == $p_hash)
    {
        //Check if it's actually the turn of our user_id

        //Add the move to the move database

        //Get the board ID
        $query = sprintf("SELECT * FROM cyvasse_games WHERE game_id = '$p_gameID'");
        $result = mysql_query($query);
        $row = mysql_fetch_assoc($result);
        $boardID = $row['board_id'];

        if ($row['game_state'] == 0)
        {
            General::ReturnSuccess(false);
            echo("The game is in setup state, how are you even moving pieces?");
            return;
        }

        if ($row['game_state'] == 3)
        {
            General::ReturnSuccess(false);
            echo("The game is in over, how are you even moving pieces?");
            return;
        }

        //Alter the board
        $fieldID = "unit_" . $p_unitID;
        mysql_query("UPDATE cyvasse_boards SET $fieldID = '$p_tileID' WHERE board_id = '$boardID'") or die (mysql_error());

        //Kill the killed unit
        if ($p_killedID != -1)
        {
            $fieldID = "unit_" . $p_killedID;
            mysql_query("UPDATE cyvasse_boards SET $fieldID = '0' WHERE board_id = '$boardID'") or die (mysql_error());
        }

        //Switch turns around, also calculate win condition here
        $newGameState = 1;
        if ($row['game_state'] == 1) { $newGameState = 2; }

        mysql_query("UPDATE cyvasse_games SET game_state = '$newGameState' WHERE game_id = '$p_gameID'") or die (mysql_error());

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
        echo("Hashes didn't match in move unit!");
    }
?>