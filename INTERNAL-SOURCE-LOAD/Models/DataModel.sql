CREATE DATABASE IF NOT EXISTS `TrainSchedule`;
USE `TrainSchedule`;

-- Listage de la structure de table TrainSchedule. trains
CREATE TABLE IF NOT EXISTS `Trains`
(
    `Id`
    int
(
    11
) NOT NULL AUTO_INCREMENT,
    `G` varchar
(
    50
) NOT NULL,
    `L` varchar
(
    50
) DEFAULT NULL,
    `Hash` varchar
(
    32
) NOT NULL COMMENT 'MD5 hash of G and L (or empty string if L is null)',
    PRIMARY KEY
(
    `Id`
),
    UNIQUE KEY `UK_Train_Hash` (`Hash`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4;

-- Trigger to automatically generate hash before insert
DELIMITER //
CREATE TRIGGER before_train_insert 
BEFORE INSERT ON Trains
FOR EACH ROW
BEGIN
    SET NEW.Hash = MD5(CONCAT(NEW.G, COALESCE(NEW.L, '')));
END//
DELIMITER ;

-- Trigger to automatically generate hash before update
DELIMITER //
CREATE TRIGGER before_train_update
BEFORE UPDATE ON Trains
FOR EACH ROW
BEGIN
    SET NEW.Hash = MD5(CONCAT(NEW.G, COALESCE(NEW.L, '')));
END//
DELIMITER ;

-- Les donnes exportes n'taient pas slectionnes.

-- Listage de la structure de table TrainSchedule. trainstations
CREATE TABLE IF NOT EXISTS `TrainStations`
(
    `Id`
    int
(
    11
) NOT NULL AUTO_INCREMENT,
    `Name` varchar
(
    255
) NOT NULL,
    PRIMARY KEY
(
    `Id`
),
    UNIQUE KEY `UK_TrainStation_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4;


-- Listage de la structure de table TrainSchedule. departures
CREATE TABLE IF NOT EXISTS `Departures`
(
    `Id`
    int
(
    11
) NOT NULL AUTO_INCREMENT,
    `DepartureStationName` varchar
(
    255
) NOT NULL,
    `DestinationStationName` varchar
(
    255
) NOT NULL,
    `ViaStationNames` text DEFAULT NULL,
    `DepartureTime` datetime NOT NULL,
    `Platform` varchar
(
    50
) NOT NULL,
    `Sector` varchar
(
    50
) DEFAULT NULL,
    `TrainStationId` int
(
    11
) DEFAULT NULL,
    `TrainId` int
(
    11
) DEFAULT NULL,
    PRIMARY KEY
(
    `Id`
),
    UNIQUE KEY `UK_Departure_Unique_Schedule` (`DepartureStationName`, `DestinationStationName`, `DepartureTime`, `Platform`),
    KEY `TrainStationId`
(
    `TrainStationId`
),
    KEY `TrainId`
(
    `TrainId`
),
    CONSTRAINT `departures_ibfk_1` FOREIGN KEY
(
    `TrainStationId`
) REFERENCES `TrainStations`
(
    `Id`
),
    CONSTRAINT `departures_ibfk_2` FOREIGN KEY
(
    `TrainId`
) REFERENCES `Trains`
(
    `Id`
)
    ) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4;

-- Les donn�es export�es n'�taient pas s�lectionn�es.
