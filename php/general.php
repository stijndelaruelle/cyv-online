<?php

class General
{
	static function Connect()
	{
		$db = mysql_connect("localhost", "blargal_main", "wafels") or die("Could not connect to database: " . mysql_error()); 
		mysql_select_db("blargal_main") or die("Could not select database: blargal_main");
		return $db;
	}

	static function SecretKey()
	{
		return "CyvasseKey";
	}

	static function ReturnSuccess($value)
	{
		if ($value == true) echo('1');
		else 			    echo ('0');
	}

	static function GetPlayerID($username)
	{
        //Check player 1
        $query = sprintf("SELECT * FROM cyvasse_users WHERE username='%s'", mysql_real_escape_string($username));
        $result = mysql_query($query);
        $row = mysql_fetch_assoc($result);
        $rowcount = mysql_num_rows($result);

        //Do we even have a record with this username?       
        if ($rowcount < 1)
        {
            return -1;
        }

        return $row['user_id'];
	}

	static function GetPlayerName($id)
	{
        //Check player 1
        $query = sprintf("SELECT * FROM cyvasse_users WHERE user_id='$id'");
        $result = mysql_query($query);
        $row = mysql_fetch_assoc($result);
        $rowcount = mysql_num_rows($result);

        //Do we even have a record with this username?       
        if ($rowcount < 1)
        {
            return -1;
        }

        return $row['username'];
	}
}

?>