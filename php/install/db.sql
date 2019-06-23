CREATE TABLE IF NOT EXISTS `users`
(
 `id`           int NOT NULL AUTO_INCREMENT ,
 `firstName`    varchar(255) NOT NULL ,
 `lastName`     varchar(255) NOT NULL ,
 `emailAddress` varchar(255) NOT NULL ,
 `username`     varchar(45) NOT NULL ,
 `password`     varchar(255) NOT NULL ,
 `ipAddress`    varchar(45) NOT NULL ,
 `staff`        int NOT NULL ,
 `banned`       int NOT NULL ,

PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `applications`
(
 `id`        int NOT NULL AUTO_INCREMENT ,
 `developer` int NOT NULL ,
 `token`     varchar(255) NOT NULL ,
 `secret`    varchar(255) NOT NULL ,
 `name`      varchar(45) NOT NULL ,

PRIMARY KEY (`id`),
KEY `fkIdx_203` (`developer`),
CONSTRAINT `FK_203` FOREIGN KEY `fkIdx_203` (`developer`) REFERENCES `users` (`id`)
);

CREATE TABLE IF NOT EXISTS `announcements`
(
 `id`          int NOT NULL ,
 `application` int NOT NULL ,
 `title`       varchar(255) NOT NULL ,
 `body`        text NOT NULL ,

PRIMARY KEY (`id`),
KEY `fkIdx_249` (`application`),
CONSTRAINT `FK_249` FOREIGN KEY `fkIdx_249` (`application`) REFERENCES `applications` (`id`)
);

CREATE TABLE IF NOT EXISTS `keyTypes`
(
 `id`   int NOT NULL AUTO_INCREMENT ,
 `name` varchar(45) NOT NULL ,

PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `keyRules`
(
 `id`        int NOT NULL AUTO_INCREMENT ,
 `developer` int NOT NULL ,
 `type`      int NOT NULL ,
 `name`      varchar(45) NOT NULL ,
 `days`      int NOT NULL ,

PRIMARY KEY (`id`),
KEY `fkIdx_235` (`developer`),
CONSTRAINT `FK_235` FOREIGN KEY `fkIdx_235` (`developer`) REFERENCES `users` (`id`),
KEY `fkIdx_240` (`type`),
CONSTRAINT `FK_240` FOREIGN KEY `fkIdx_240` (`type`) REFERENCES `keyTypes` (`id`)
);

CREATE TABLE IF NOT EXISTS `keys`
(
 `id`          int NOT NULL AUTO_INCREMENT ,
 `application` int NOT NULL ,
 `rule`        int NOT NULL ,
 `value`       varchar(45) NOT NULL ,
 `tracking`    text NOT NULL ,
 `expiration`  timestamp NOT NULL ,
 `activated`   int NOT NULL ,
 `banned`      int NOT NULL ,

PRIMARY KEY (`id`),
KEY `fkIdx_214` (`application`),
CONSTRAINT `FK_214` FOREIGN KEY `fkIdx_214` (`application`) REFERENCES `applications` (`id`),
KEY `fkIdx_243` (`rule`),
CONSTRAINT `FK_243` FOREIGN KEY `fkIdx_243` (`rule`) REFERENCES `keyRules` (`id`)
);