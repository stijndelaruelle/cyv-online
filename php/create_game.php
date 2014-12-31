<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    // Strings must be escaped to prevent SQL injection attack. 
    $p_player_white = mysql_real_escape_string($_GET["p1"], $db); 
    $p_player_black = mysql_real_escape_string($_GET["p2"], $db); 

    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = md5($p_player_white . $p_player_black . General::SecretKey()); //Create our own hash

    if($real_hash == $p_hash)
    {
        //Get white player ID
        $white_id = General::GetPlayerID($p_player_white);
        if ($white_id == -1)
        {
            General::ReturnSuccess(false);
            echo("White player not found!");
            return;
        }

        //Get black player ID
        $black_id = General::GetPlayerID($p_player_black);
        if ($white_id == -1)
        {
            General::ReturnSuccess(false);
            echo("White player not found!");
            return;
        }

        if ($white_id == $black_id)
        {
            General::ReturnSuccess(false);
            echo("You are trying to challenge yourself!");
        }

        //Create board

        //Create game
        $currentDate = date('Y-m-d H:i:s');

        mysql_query("INSERT INTO cyvasse_games (game_id, player_white_id, player_black_id, board_id, create_date, last_move_date)
                            VALUES('', '$white_id', '$black_id', 0, '$currentDate', '$currentDate')")
                            or die("Query failed: " . mysql_error());

        //Set both players to update
        mysql_query("UPDATE cyvasse_users SET update = '1' WHERE user_id = '$white_id'") or die (mysql_error());
        mysql_query("UPDATE cyvasse_users SET update = '1' WHERE user_id = '$black_id'") or die (mysql_error());

        General::ReturnSuccess(true);
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match!");
    }
?>