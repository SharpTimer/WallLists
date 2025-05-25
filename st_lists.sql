SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

-- --------------------------------------------------------

--
-- Table structure for table `st_lists`
--

CREATE TABLE `st_lists` (
  `MapName` varchar(255) NOT NULL,
  `ListType` varchar(50) NOT NULL,
  `Location1` text,
  `Location2` text,
  `Location3` text,
  `Location4` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `st_lists`
--

INSERT INTO `st_lists` (`MapName`, `ListType`, `Location1`, `Location2`, `Location3`, `Location4`) VALUES
('surf_ace', 'Completions', '-1790.97 271.71 3340.87,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_ace', 'Points', '-2290.16 29.79 3979.87,0.00 270.00 90.00', '-1790.97 -354.15 3339.87,0.00 90.00 90.00', NULL, NULL),
('surf_ace', 'Times', '-2293.17 -286.09 3979.87,0.00 270.00 90.00', '-1790.97 101.71 3339.87,0.00 90.00 90.00', NULL, NULL),
('surf_adventure', 'Completions', '-11570.00 -13259.00 12175.00,0.00 190.00 90.00', NULL, NULL, NULL),
('surf_adventure', 'Points', '-11452.60 -13220.00 12175.00,0.00 190.00 90.00', NULL, NULL, NULL),
('surf_adventure', 'Times', '-11341.04 -13200.00 12175.00,0.00 190.00 90.00', NULL, NULL, NULL),
('surf_aquaflow', 'Points', '-9905.88 -1732.68 6865.00,0.00 220.00 90.00', NULL, NULL, NULL),
('surf_aquaflow', 'Times', '-10699.36 -1735.21 6865.00,0.00 150.00 90.00', NULL, NULL, NULL),
('surf_artois', 'Points', '-851.10 15078.97 -675.13,0.00 360.00 90.00', '-16254.97 13862.84 108.87,0.00 90.00 90.00', NULL, NULL),
('surf_artois', 'Times', '-346.88 15078.97 -675.13,0.00 360.00 90.00', '-16254.97 14064.06 108.87,0.00 90.00 90.00', NULL, NULL),
('surf_astra', 'Points', '-12201.03 -6948.87 15539.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_astra', 'Times', '-12201.03 -7418.31 15539.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_atrium', 'Points', '-13510.17 5453.10 13240.00,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_atrium', 'Times', '-13510.17 5858.98 13240.00,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_aura', 'Completions', '-15727.92 15033.63 1069.03,0.00 450.00 90.00', '1801.79 -1534.11 -371.48,0.00 230.00 90.00', NULL, NULL),
('surf_aura', 'Points', '1718.51 -1610.56 -370.97,0.00 180.00 90.00', '-15585.18 15502.81 1068.87,0.00 360.00 90.00', '-15726.55 15150.58 1068.87,0.00 450.00 90.00', '5382.64 1110.81 -3283.13,0.00 270.00 90.00'),
('surf_aura', 'Times', '1455.89 -1610.56 -370.97,0.00 180.00 90.00', '-15330.40 15421.69 1068.87,0.00 310.00 90.00', '-15336.34 14807.49 1068.87,0.00 220.00 90.00', '5381.89 1439.27 -3283.13,0.00 270.00 90.00'),
('surf_beginner', 'Completions', '-116.05 -62.97 428.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_beginner', 'Points', '-57.85 2629.79 159.39,0.00 180.00 90.00', '136.79 -62.97 427.87,0.00 180.00 90.00', NULL, NULL),
('surf_beginner', 'Times', '-443.84 2629.79 159.39,0.00 180.00 90.00', '-372.78 -62.97 428.87,0.00 180.00 90.00', NULL, NULL),
('surf_beginner2', 'Completions', '-16227.33 15473.75 15688.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_beginner2', 'Points', '-16318.95 15781.17 15688.87,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_beginner2', 'Times', '-16206.70 15886.92 15688.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_benevolent', 'Completions', '-894.97 -15393.50 13484.87,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_benevolent', 'Points', '-5473.59 11925.42 -11687.13,0.00 270.00 90.00', '-894.97 -15639.98 13483.87,0.00 450.00 90.00', NULL, NULL),
('surf_benevolent', 'Times', '-5468.57 11510.35 -11687.13,0.00 270.00 90.00', '-894.97 -15083.80 13483.87,0.00 90.00 90.00', NULL, NULL),
('surf_borderlands', 'Completions', '-11386.31 -197.91 -11417.11,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_borderlands', 'Points', '-12197.59 -191.84 -11417.11,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_borderlands', 'Times', '-11461.25 -343.33 -11417.27,0.00 220.00 90.00', '-12123.24 -342.84 -11417.27,0.00 130.00 90.00', NULL, NULL),
('surf_boreas', 'Points', '12993.96 -12486.70 14887.29,0.00 200.00 90.00', NULL, NULL, NULL),
('surf_boreas', 'Times', '12603.88 -12445.18 14887.29,0.00 160.00 90.00', '13030.45 -11845.84 14844.87,0.00 310.00 90.00', NULL, NULL),
('surf_capsule', 'Completions', '15358.97 7050.46 4516.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_capsule', 'Points', '15358.97 7305.61 4516.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_capsule', 'Times', '15053.61 6886.98 4516.87,0.00 130.00 90.00', '15047.05 7466.94 4516.87,0.00 400.00 90.00', NULL, NULL),
('surf_colum_2', 'Completions', '153.50 -1022.97 1644.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_colum_2', 'Points', '307.94 -1022.97 1644.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_colum_2', 'Times', '441.37 -1022.97 1644.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_craneworks', 'Times', '-12183.58 -8424.93 9901.78,0.00 310.00 90.00', '-12184.86 -9031.77 9901.78,0.00 220.00 90.00', NULL, NULL),
('surf_dojo', 'Completions', '-12700.50 -7086.97 492.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_dojo', 'Points', '-12415.76 -6513.27 525.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_dojo', 'Times', '-12403.04 -7278.97 525.00,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_easy2', 'Completions', '2542.97 -3169.47 3431.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_easy2', 'Points', '2542.97 -3761.01 3431.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_easy2', 'Times', '2095.65 -3829.91 3431.87,0.00 130.00 90.00', '2098.93 -3163.76 3431.87,0.00 410.00 90.00', NULL, NULL),
('surf_enlightened', 'Points', '14511.43 -9443.77 7884.87,0.00 300.00 90.00', NULL, NULL, NULL),
('surf_enlightened', 'Times', '14599.44 -9929.54 7884.87,0.00 260.00 90.00', NULL, NULL, NULL),
('surf_evo', 'Completions', '-411.22 220.71 983.86,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_evo', 'Points', '-70.45 798.87 940.87,0.00 320.00 90.00', NULL, NULL, NULL),
('surf_evo', 'Times', '-74.75 321.92 940.87,0.00 220.00 90.00', NULL, NULL, NULL),
('surf_ezpz_syntax', 'Completions', '-135.53 3750.35 -339.13,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_ezpz_syntax', 'Points', '-135.53 4070.87 -340.00,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_ezpz_syntax', 'Times', '-135.53 3908.65 -340.00,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_first', 'Completions', '7029.29 -3152.27 1387.44,0.00 450.00 90.00', NULL, NULL, NULL),
('surf_first', 'Points', '7030.14 -2994.63 1387.44,0.00 450.00 90.00', NULL, NULL, NULL),
('surf_first', 'Times', '7117.77 -2785.49 1387.44,0.00 410.00 90.00', '7987.64 -2783.78 1387.60,0.00 310.00 90.00', NULL, NULL),
('surf_garden', 'Completions', '-15806.97 12328.94 3212.87,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_garden', 'Points', '101.73 1940.23 3544.87,0.00 320.00 90.00', '-15492.29 12225.03 3212.87,0.00 180.00 90.00', NULL, NULL),
('surf_garden', 'Times', '103.29 1197.34 3544.87,0.00 220.00 90.00', '-15659.01 12225.03 3212.87,0.00 180.00 90.00', NULL, NULL),
('surf_gleam', 'Completions', '-1854.88 -3136.50 8249.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_gleam', 'Points', '-2171.36 -3136.38 8250.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_gleam', 'Times', '-2021.57 -3136.30 8250.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_het', 'Times', '-8388.32 -12969.93 13708.87,0.00 400.00 90.00', NULL, NULL, NULL),
('surf_hologram2', 'Completions', '-15361.53 467.24 14460.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_hologram2', 'Points', '-15136.38 464.72 14460.87,0.00 360.00 90.00', '-15213.08 462.44 14460.87,0.00 360.00 90.00', '-15195.56 484.83 14460.87,0.00 360.00 90.00', NULL),
('surf_hologram2', 'Times', '-15034.22 458.63 14460.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_how2surf', 'Completions', '-14721.01 14352.03 220.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_how2surf', 'Points', '-14671.89 14609.74 220.87,0.00 320.00 90.00', NULL, NULL, NULL),
('surf_how2surf', 'Times', '-14997.28 14606.59 220.87,0.00 410.00 90.00', '-15017.88 14622.42 221.03,0.00 410.00 90.00', NULL, NULL),
('surf_hyzer', 'Points', '-13731.21 1186.60 8486.63,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_hyzer', 'Times', '-13955.45 1185.05 8486.63,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_intro', 'Points', '1511.66 6282.91 3564.87,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_intro', 'Times', '1516.81 6641.63 3564.87,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_ivory', 'Completions', '2928.98 12702.94 15754.57,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_ivory', 'Points', '2761.39 12691.07 15754.57,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_ivory', 'Times', '2777.89 12272.91 15746.57,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_juturna', 'Points', '14597.03 -192.11 15405.00,0.00 450.00 90.00', '4207.20 125.83 4947.00,0.00 270.00 90.00', NULL, NULL),
('surf_juturna', 'Times', '14597.03 192.37 15405.00,0.00 90.00 90.00', '4207.20 -129.13 4947.00,0.00 270.00 90.00', '3640.08 -291.32 4942.95,0.00 130.00 90.00', NULL),
('surf_kitsune', 'Completions', '391.71 -513.03 204.87,0.00 360.00 90.00', '-15462.85 -15358.97 924.87,0.00 180.00 90.00', NULL, NULL),
('surf_kitsune', 'Points', '-246.26 -513.03 203.87,0.00 360.00 90.00', '-15104.39 -15358.97 923.87,0.00 180.00 90.00', NULL, NULL),
('surf_kitsune', 'Times', '-15614.16 -15358.97 923.87,0.00 180.00 90.00', '250.56 -513.03 203.87,0.00 360.00 90.00', NULL, NULL),
('surf_leet_xl_beta7z', 'Points', '-5699.87 -11233.00 15003.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_leet_xl_beta7z', 'Times', '-5848.66 -11233.00 15003.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_lockdown', 'Completions', '-10837.03 -510.97 -6547.13,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_lockdown', 'Points', '-10511.42 -510.97 -6548.00,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_lockdown', 'Times', '-10662.10 -510.97 -6548.00,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_lux', 'Completions', '16.63 2430.97 1708.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_lux', 'Points', '-127.37 2430.97 1708.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_lux', 'Times', '341.28 1963.06 1708.87,0.00 220.00 90.00', '-346.84 1959.42 1708.87,0.00 140.00 90.00', NULL, NULL),
('surf_me', 'Completions', '-14189.47 -2647.61 1196.87,0.00 450.00 90.00', NULL, NULL, NULL),
('surf_me', 'Points', '-14749.09 13180.59 1755.16,0.00 450.00 90.00', '-14189.47 -3497.89 1195.87,0.00 450.00 90.00', NULL, NULL),
('surf_me', 'Times', '-14747.01 13322.51 1755.16,0.00 90.00 90.00', '-12914.33 -2768.36 1196.87,0.00 320.00 90.00', '-12903.02 -3408.93 1196.87,0.00 220.00 90.00', NULL),
('surf_mesa_revo', 'Points', '540.20 -1254.87 9036.87,0.00 240.00 90.00', NULL, NULL, NULL),
('surf_mesa_revo', 'Times', '-550.41 -1272.27 9036.87,0.00 120.00 90.00', NULL, NULL, NULL),
('surf_minecraft_2016_final', 'Completions', '-16280.29 -15809.03 524.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_minecraft_2016_final', 'Points', '-16111.23 -15809.03 524.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_minecraft_2016_final', 'Times', '-15913.92 -15746.50 525.03,0.00 310.00 90.00', NULL, NULL, NULL),
('surf_mom', 'Points', '-3614.97 -202.54 620.87,0.00 450.00 90.00', NULL, NULL, NULL),
('surf_mom', 'Times', '-3614.97 -17.29 620.87,0.00 450.00 90.00', NULL, NULL, NULL),
('surf_network_2009', 'Points', '-510.97 -214.99 140.87,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_network_2009', 'Times', '510.97 -221.72 140.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_nova', 'Completions', '-170.50 1985.84 13196.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_nova', 'Points', '166.95 1986.59 13196.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_nova', 'Times', '-8.72 1986.82 13196.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_nyx', 'Completions', '14406.55 -8240.86 2179.78,0.00 280.00 90.00', NULL, NULL, NULL),
('surf_nyx', 'Points', '-11623.36 -14799.02 -7519.58,0.00 120.00 90.00', '14441.92 -8423.87 2181.03,0.00 270.00 90.00', NULL, NULL),
('surf_nyx', 'Times', '-11619.46 -14470.75 -7519.58,0.00 420.00 90.00', '13730.24 -8438.53 2181.03,0.00 90.00 90.00', NULL, NULL),
('surf_palais', 'Completions', '-9726.97 4084.90 10924.87,0.00 450.00 90.00', '-9726.97 4033.90 10924.87,0.00 450.00 90.00', '-9726.97 3123.08 10924.87,0.00 450.00 90.00', NULL),
('surf_palais', 'Points', '-9726.95 3833.82 10924.87,0.00 450.00 90.00', '-9726.90 3861.52 10924.87,0.00 450.00 90.00', '-9726.97 3910.51 10924.87,0.00 450.00 90.00', '-9726.97 3272.24 10924.87,0.00 450.00 90.00'),
('surf_palais', 'Times', '-9213.16 3884.60 10924.87,0.00 310.00 90.00', '-9211.84 3339.64 10924.87,0.00 230.00 90.00', NULL, NULL),
('surf_pantheon', 'Completions', '10532.06 781.82 14830.53,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_pantheon', 'Points', '10369.33 799.68 14830.37,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_pantheon', 'Times', '10366.50 1577.76 14830.37,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_prisma', 'Points', '16214.00 15844.56 12636.87,0.00 270.00 90.00', '-16270.25 416.50 15782.89,0.00 450.00 90.00', NULL, NULL),
('surf_prisma', 'Times', '16214.00 15359.86 12636.87,0.00 270.00 90.00', '-16270.25 591.46 15782.89,0.00 450.00 90.00', NULL, NULL),
('surf_psycho', 'Completions', '-9941.88 -13941.03 -14485.13,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_psycho', 'Points', '-10218.47 -13941.03 -14486.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_psycho', 'Times', '-10075.12 -13941.03 -14486.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_race', 'Points', '-8321.03 -8459.29 14443.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_race', 'Times', '-8321.03 -8681.48 14443.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_ravine', 'Points', '14985.95 4107.43 6764.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_ravine', 'Times', '14865.63 4105.90 6764.87,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_reprise', 'Points', '-3264.00 -1150.00 14477.00,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_reprise', 'Times', '-3135.00 -1150.00 14477.00,0.00 180.00 90.00', NULL, NULL, NULL),
('surf_rookie', 'Points', '-4924.15 13195.75 15183.06,0.00 410.00 90.00', '-1245.90 10831.07 13788.87,0.00 230.00 90.00', NULL, NULL),
('surf_rookie', 'Times', '-4593.76 13181.26 15183.06,0.00 320.00 90.00', '-1227.22 11454.35 13787.78,0.00 320.00 90.00', NULL, NULL),
('surf_salvador', 'Completions', '8114.14 -703.91 1900.87,0.00 400.00 90.00', NULL, NULL, NULL),
('surf_salvador', 'Points', '8413.25 -641.03 1901.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_salvador', 'Times', '8275.91 -641.03 1901.00,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_sandtrap2', 'Points', '3039.56 -11233.03 10604.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_sandtrap2', 'Times', '3553.96 -11233.03 10564.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_santorini_ksf', 'Points', '-10817.03 -10728.94 876.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_santorini_ksf', 'Times', '-10817.03 -10876.91 876.87,0.00 270.00 90.00', NULL, NULL, NULL),
('surf_simpsons_cs2', 'Completions', '3994.57 4071.47 -83.13,0.00 130.00 90.00', '4356.77 3777.03 -83.13,0.00 180.00 90.00', NULL, NULL),
('surf_simpsons_cs2', 'Points', '3905.03 4414.48 -83.13,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_simpsons_cs2', 'Times', '3905.03 4230.54 -91.13,0.00 90.00 90.00', NULL, NULL, NULL),
('surf_skipalot', 'Completions', '-3998.54 787.14 13548.87,0.00 140.00 90.00', '-5549.30 473.81 14252.87,0.00 360.00 90.00', '-5556.12 -429.44 14252.87,0.00 180.00 90.00', NULL),
('surf_skipalot', 'Points', '-3552.71 645.44 13549.00,0.00 230.00 90.00', '-5688.85 471.75 14252.87,0.00 360.00 90.00', '-5680.65 -427.08 14252.87,0.00 180.00 90.00', NULL),
('surf_skipalot', 'Times', '-3899.34 644.46 13549.00,0.00 140.00 90.00', '-5874.01 -327.09 14252.87,0.00 140.00 90.00', '-5880.83 337.37 14252.87,0.00 410.00 90.00', NULL),
('surf_tranquil', 'Completions', '4614.82 3024.64 7654.87,0.00 370.00 90.00', '-2506.44 1462.97 4426.87,0.00 360.00 90.00', NULL, NULL),
('surf_tranquil', 'Points', '4485.99 2969.05 7655.00,0.00 390.00 90.00', '-2366.76 1462.97 4427.00,0.00 360.00 90.00', NULL, NULL),
('surf_tranquil', 'Times', '4761.07 2973.33 7655.00,0.00 330.00 90.00', '-2246.15 1462.97 4427.00,0.00 360.00 90.00', NULL, NULL),
('surf_trazhtec', 'Points', '-775.27 13212.50 11048.43,0.00 450.00 90.00', NULL, NULL, NULL),
('surf_trazhtec', 'Times', '-775.27 13325.73 11048.43,0.00 450.00 90.00', NULL, NULL, NULL),
('surf_utopia_njv', 'Completions', '-14334.97 -511.35 12908.87,0.00 90.00 90.00', '-14334.97 502.60 12908.87,0.00 90.00 90.00', NULL, NULL),
('surf_utopia_njv', 'Points', '-14334.97 -383.92 12908.87,0.00 90.00 90.00', '-14334.97 386.58 12908.87,0.00 90.00 90.00', NULL, NULL),
('surf_utopia_njv', 'Times', '-13802.79 272.60 12908.87,0.00 300.00 90.00', '-13827.86 -260.67 12908.87,0.00 230.00 90.00', NULL, NULL),
('surf_volvic', 'Points', '-1972.97 4634.00 -7196.13,0.00 90.00 90.00', '-3950.47 9572.16 8317.37,0.00 90.00 90.00', NULL, NULL),
('surf_volvic', 'Times', '-1972.97 5067.74 -7196.13,0.00 90.00 90.00', '-3950.47 9858.13 8317.37,0.00 90.00 90.00', NULL, NULL),
('surf_whiteout', 'Points', '9656.53 -13810.30 15788.87,0.00 360.00 90.00', NULL, NULL, NULL),
('surf_whiteout', 'Times', '9803.15 -13810.30 15788.87,0.00 360.00 90.00', NULL, NULL, NULL);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `st_lists`
--
ALTER TABLE `st_lists`
  ADD PRIMARY KEY (`MapName`,`ListType`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
