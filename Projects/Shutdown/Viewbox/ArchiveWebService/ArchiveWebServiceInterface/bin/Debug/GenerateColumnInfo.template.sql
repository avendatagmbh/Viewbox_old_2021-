-- --------------------------------------------------------------------------------
-- Routine DDL
-- Note: comments before and after the routine body will not be stored by the server
-- --------------------------------------------------------------------------------

CREATE PROCEDURE `GenerateColumnInfo`(IN InColumnId INT)
BEGIN	
	DECLARE loop0_eof INT DEFAULT FALSE; 
	DECLARE loop0_column_id INT; 
	DECLARE loop0_table_id INT; 
	DECLARE row_count INT; 
	DECLARE loop0_column_name VARCHAR(255); 
	DECLARE loop0_table_name VARCHAR(255); 
	DECLARE loop0_database_name VARCHAR(255); 
	DECLARE loop0_data_type INT; 
	DECLARE loop0_max_length INT; 
	DECLARE coll_id INT;
	DECLARE cur0 CURSOR FOR 
		SELECT c.`id`, t.`id`, LOWER(c.`name`), t.`name`, t.`database`, c.`data_type`, c.`max_length`
			FROM 
				`viewbox_zsv`.`columns` c
			JOIN `viewbox_zsv`.`tables` t ON
				t.`id`=c.`table_id`
			WHERE  c.`id`=InColumnId;


	DECLARE CONTINUE HANDLER FOR NOT FOUND SET loop0_eof = TRUE;

	
	OPEN cur0; 
		loop0: LOOP 
			FETCH cur0 INTO loop0_column_id, loop0_table_id, loop0_column_name, loop0_table_name,loop0_database_name,loop0_data_type,loop0_max_length;
			IF loop0_eof THEN
				leave loop0;
			END IF;


						SET @type = "";
						-- loop0_max_length
						SET @string_length = loop0_max_length;
						
						if (loop0_data_type = 0) then
							CASE @string_length
							WHEN 0 THEN SET @string_length = 128;
							ELSE SET @string_length = loop0_max_length;
						END CASE;
						end if;
												
						CASE loop0_data_type
							WHEN 0 THEN SET @type = CONCAT("VARCHAR(", @string_length, ")");	-- ok
							WHEN 1 THEN SET @type = "INT";		-- ok
							WHEN 2 THEN SET @type = CONCAT("DECIMAL(25, ", @string_length, ")"); -- max 65, max 30
							WHEN 3 THEN SET @type = CONCAT("DECIMAL(25, ", @string_length, ")"); -- max 65, max 30
							WHEN 4 THEN SET @type = "DATE";			-- ok
							WHEN 5 THEN SET @type = "TINYINT(1)";	-- ok
							WHEN 8 THEN SET @type = "TIME";			-- ok
							WHEN 12 THEN SET @type = "DATETIME";		-- ok
							ELSE SET @type = @type = "'VARCHAR(128)";	-- ok
						END CASE;
						
						
						SET @sss11=CONCAT("
										DROP TABLE IF EXISTS `value_",loop0_column_id,"`;
										");
						PREPARE stmt11 FROM @sss11;
						EXECUTE stmt11;
						
						
						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `temp_index_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;
						
						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `index_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;

						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `temp_order_areas_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;
						
						SET @sss1=CONCAT("										
										CREATE TABLE `value_",loop0_column_id,"` ( 
										  `id` INT,
										  `value` ", @type, " DEFAULT NULL
										) ENGINE=MyISAM;
										");

						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;


						SET @sss1=CONCAT("										
										CREATE TABLE `temp_order_areas_",loop0_column_id,"` ( 
										  `id` INT NOT NULL,
										  `start` bigINT,
										  `end` bigINT,
										  PRIMARY KEY (`id`)
										) ENGINE=MyISAM;
										");

						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;

						SET @sss1=CONCAT("										
										insert into `temp_order_areas_",loop0_column_id,"` ( `id`, `start`, `end`)
										select `id`, `start`, `end` from viewbox_zsv.order_areas 
										where table_id=",loop0_table_id," ;
										");

						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;

						SET @sss2=CONCAT("										
										CREATE TABLE `temp_index_",loop0_column_id,"` (
										  `value_id` INT,
										  `order_areas_id` INT
										) ENGINE=MyISAM;
										");
						PREPARE stmt2 FROM @sss2;
						EXECUTE stmt2;


						SET @sss4=CONCAT("
										ALTER TABLE `temp_order_areas_",loop0_column_id,"` 
										ADD INDEX `all` (`id` ASC, `start` ASC, `end` ASC),
										ADD INDEX `all2` (`start` ASC, `end` ASC, `id` ASC);
										");
						PREPARE stmt4 FROM @sss4;
						EXECUTE stmt4;
						
						
						BLOCK_DONE: BEGIN
							DECLARE failed INT DEFAULT FALSE;
							DECLARE CONTINUE HANDLER FOR SQLEXCEPTION SET failed = TRUE;
							  BEGIN 
							  
						SET @sss11=CONCAT("
										DROP TABLE IF EXISTS `TEMP_value_",loop0_column_id,"`;
										");
						PREPARE stmt11 FROM @sss11;
						EXECUTE stmt11;
						
						SET @sss1=CONCAT("										
										CREATE TABLE `temp_value_",loop0_column_id,"` ( 
										  `id` INT,
										  `value` ", @type, " DEFAULT NULL
										) ENGINE=MyISAM;
										");
						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;
						
								SET @sss3=CONCAT("
									INSERT INTO `TEMP_value_",loop0_column_id,"` 
									(`id`,`value`)
									SELECT _row_no_, `",loop0_column_name,"`
									FROM `",loop0_database_name,"`.`",loop0_table_name,"`
									WHERE COALESCE(`",loop0_column_name,"`, '') <> '';
									");
								PREPARE stmt3 FROM @sss3;
								EXECUTE stmt3;
							  END;
							IF failed THEN
							
							
						SET @sss11=CONCAT("
										DROP TABLE IF EXISTS `TEMP_value_",loop0_column_id,"`;
										");
						PREPARE stmt11 FROM @sss11;
						EXECUTE stmt11;
						
						SET @sss1=CONCAT("										
										CREATE TABLE `temp_value_",loop0_column_id,"` ( 
										  `id` INT NOT NULL AUTO_INCREMENT,
										  `value` ", @type, " DEFAULT NULL,
										  PRIMARY KEY (`id`)
										) ENGINE=MyISAM;
										");
						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;
						
							 SET @sss3=CONCAT("
									INSERT INTO `TEMP_value_",loop0_column_id,"` 
									(`value`)
									SELECT `",loop0_column_name,"`
									FROM `",loop0_database_name,"`.`",loop0_table_name,"`
									WHERE COALESCE(`",loop0_column_name,"`, '') <> '';
									");
									PREPARE stmt3 FROM @sss3;
									EXECUTE stmt3;
							END IF;
						END BLOCK_DONE;
						
						
						  
						SET @sss4=CONCAT("
										ALTER TABLE `temp_value_",loop0_column_id,"` 
										ADD INDEX `valueid` (`value` ASC, `id` ASC);
										");
						PREPARE stmt4 FROM @sss4;
						EXECUTE stmt4;
						
						SET @sss1=CONCAT("
										INSERT INTO value_",loop0_column_id,"(`id`, `VALUE`)
										SELECT MIN(v.`ID`), V.`VALUE`
										FROM `TEMP_VALUE_",loop0_column_id,"` v 
										GROUP BY 2");
						PREPARE stmt22 FROM @sss1;
						EXECUTE stmt22;
						
						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `index_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;

						
						SET @sss4=CONCAT("
										ALTER TABLE `value_",loop0_column_id,"` 
										ADD INDEX `ALL` (`value` ASC, `id` ASC);");
										
						PREPARE stmt22 FROM @sss4;
						EXECUTE stmt22;
						
						
						SET @sss1=CONCAT("
										INSERT INTO TEMP_INDEX_",loop0_column_id,"(`value_id`, `order_areas_id`)
										SELECT distinct v2.`id`, o.`id`
										FROM `TEMP_VALUE_",loop0_column_id,"` v 
											JOIN `temp_order_areas_",loop0_column_id,"` O ON v.`ID` >= O.`START` AND v.`ID` <= O.`END`
											JOIN `VALUE_",loop0_column_id,"` V2 on v2.`value` = v.`value`
										");
						PREPARE stmt22 FROM @sss1;
						EXECUTE stmt22;
						
						
						SET @sss2=CONCAT("
										ALTER TABLE  `temp_index_",loop0_column_id,"` 
											RENAME TO `index_",loop0_column_id,"`,
											ADD INDEX `value_id` (`value_id` ASC),
											ADD INDEX `order_areas_id` (`order_areas_id` ASC);
										");
						PREPARE stmt2 FROM @sss2;
						EXECUTE stmt2;
						
						SET @sss11=CONCAT("
										DROP TABLE IF EXISTS `TEMP_value_",loop0_column_id,"`;
										");
						PREPARE stmt11 FROM @sss11;
						EXECUTE stmt11;
						
						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `temp_index_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;
						
						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `temp_order_areas_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;
						
	END LOOP; 
	CLOSE cur0; 

END