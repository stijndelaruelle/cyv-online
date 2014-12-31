<?php
    include('general.php');

    //Database connection
    $db = General::Connect();
 
    $result = mysql_query("SELECT * FROM cyvasse_users") or die("Query failed: " . mysql_error());
 
    $num_results = mysql_num_rows($result);  
 
    for($i = 0; $i < $num_results; ++$i)
    {
         $row = mysql_fetch_array($result);
         echo $row["username"] . "\t" . $row["password"] . "\n";
    }
?>