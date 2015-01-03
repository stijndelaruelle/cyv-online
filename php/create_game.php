<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    // Strings must be escaped to prevent SQL injection attack. 
    $p_white_id = mysql_real_escape_string($_GET["p1"], $db); 
    $p_black_id = mysql_real_escape_string($_GET["p2"], $db); 

    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = md5($p_white_id . $p_black_id . General::SecretKey()); //Create our own hash

    if($real_hash == $p_hash)
    {
        if ($p_white_id == $p_black_id)
        {
            General::ReturnSuccess(false);
            echo("You are trying to challenge yourself!");
        }

        //Create board
        $result = mysql_query("INSERT INTO cyvasse_boards (board_id) VALUES ('')");
        $board_id = mysql_insert_id();

        //Create game
        $currentDate = date('Y-m-d H:i:s');

        $result = mysql_query("INSERT INTO cyvasse_games (game_id, user_id_white, user_id_black, board_id, last_move_date, create_date)
                                      VALUES('', '$p_white_id', '$p_black_id', '$board_id', '$currentDate', '$currentDate')")
                                      or die("Query failed: " . mysql_error());

        if (!$result)
        {
            General::ReturnSuccess(false);
            return;
        }

        //Set both players to update
        mysql_query("UPDATE cyvasse_users SET refresh = '1' WHERE user_id = '$p_white_id'") or die (mysql_error());
        mysql_query("UPDATE cyvasse_users SET refresh = '1' WHERE user_id = '$p_black_id'") or die (mysql_error());

        General::ReturnSuccess(true);
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match in create game!");
    }
?>