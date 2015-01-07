<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    // Strings must be escaped to prevent SQL injection attack.
    $p_ID = mysql_real_escape_string($_GET["p"], $db); 
    $p_gameID = mysql_real_escape_string($_GET["b"], $db); 

    //Compare hashes
    $p_hash = $_GET["hash"]; //Hash generated from the application
    $real_hash = md5($p_ID . $p_gameID . General::SecretKey()); //Create our own hash

    if($real_hash == $p_hash)
    {
        //Get all the games the player is playing
        $query = sprintf("SELECT * FROM cyvasse_games WHERE game_id = '$p_gameID'");
        $result = mysql_query($query);
        $row = mysql_fetch_assoc($result);
        $rowcount = mysql_num_rows($result);

        if ($rowcount == 0)
        {
            General::ReturnSuccess(false);
            echo("Game doesn't even exist (game id: " . $p_gameID . ")");
        }

        if ($p_ID !== $row['user_id_white'] && $p_ID != $row['user_id_black'])
        {
            General::ReturnSuccess(false);
            echo("User isn't even part of this game (user id: " . $p_ID . ")");
        }

        $isPlayerBlack = false;
        if ($p_ID == $row['user_id_black']) $isPlayerBlack = true;

        $isInSetupMode = false;
        if ($row['game_state'] == 0) $isInSetupMode = true;

        //Get the board
        $boardID = $row['board_id'];
        $query = sprintf("SELECT * FROM cyvasse_boards WHERE board_id = '$boardID'");
        $result = mysql_query($query);
        $row = mysql_fetch_assoc($result);
        $rowcount = mysql_num_rows($result);

        if ($rowcount == 0)
        {
            General::ReturnSuccess(false);
            echo("board doesn't even exist (bpard id: " . $boardID . ")");
        }

        General::ReturnSuccess(true);

        //Return all the players units (don't send the other player's units to avoid cheating)
        $beginUnitID = 0;
        $endUnitID = 52;

        if ($isInSetupMode)
        {
            if ($isPlayerBlack) $beginUnitID = 26;
            $endUnitID = $beginUnitID + 26;
        }

        for ($i = $beginUnitID; $i < $endUnitID; ++$i)
        {
            echo($row['unit_' . $i]); echo("/");
        }
    }
    else
    {
        General::ReturnSuccess(false);
        echo("Hashes didn't match in get board!");
    }
?>