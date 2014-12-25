<?php
    //Database connection
    $db = mysql_connect("localhost", "blargal_main", "wafels") or die("Could not connect to database: " . mysql_error()); 
    mysql_select_db("blargal_main") or die("Could not select database: blargal_main");
 
    $result = mysql_query("SELECT * FROM cyvasse_users") or die("Query failed: " . mysql_error());
 
    $num_results = mysql_num_rows($result);  
 
    for($i = 0; $i < $num_results; ++$i)
    {
         $row = mysql_fetch_array($result);
         echo $row["username"] . "\t" . $row["password"] . "\n";
    }
?>