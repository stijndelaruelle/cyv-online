<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    // Strings must be escaped to prevent SQL injection attack.
    $p_id = mysql_real_escape_string($_GET["p"], $db); 


    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = md5($p_id . General::SecretKey()); //Create our own hash

    if($real_hash == $p_hash)
    {
        //Get all the games the player is playing
        $query = sprintf("SELECT * FROM cyvasse_games WHERE user_id_white = '$p_id' OR user_id_black = '$p_id' ORDER BY last_move_date");
        $result = mysql_query($query);

        General::ReturnSuccess(true);

        while ($row = mysql_fetch_assoc($result))
        {
            echo($row['game_id']);
            echo("/"); //add separator

            //Who's turn it is, get that from the board
            echo("0");
            echo("/"); //add separator

            //Get the username of the opponent
            if ($p_id == $row['user_id_white'])
            {
                echo("0"); //We are white
                echo("/"); //add separator
                echo(General::GetPlayerName($row['user_id_black']));
            }
            else
            {
                echo("1"); //We are black
                echo("/"); //add separator
                echo(General::GetPlayerName($row['user_id_white']));
            }
            echo("/"); //add separator
        }

        //We have updated, set refresh to false
        mysql_query("UPDATE cyvasse_users SET refresh = '0' WHERE user_id = '$p_id'") or die (mysql_error());
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match in get games!");
    }
?>