INSERT INTO HRConnect.dbo.MedicalOptions (MedicalOptionName,MedicalOptionCategoryId,SalaryBracketMin,SalaryBracketMax,MonthlyMsaContributionAdult,MonthlyMsaContributionChild,MonthlyMsaContributionPrincipal,MonthlyRiskContributionAdult,MonthlyRiskContributionChild,MonthlyRiskContributionChild2,MonthlyRiskContributionPrincipal,TotalMonthlyContributionsAdult,TotalMonthlyContributionsChild,TotalMonthlyContributionsChild2,TotalMonthlyContributionsPrincipal) VALUES
	 (N'Network Choice 1',1,0.00,20000.00,NULL,NULL,NULL,1850.00,1925.00,0.00,1850.00,1850.00,1925.00,0.00,1850.00),
	 (N'Network Choice 2',1,20001.00,24710.00,NULL,NULL,NULL,2070.00,1090.00,0.00,2465.00,2070.00,1090.00,0.00,2465.00),
	 (N'Network Choice 3',1,24711.00,33120.00,NULL,NULL,NULL,2345.00,1345.00,0.00,2940.00,2345.00,1345.00,0.00,2940.00),
	 (N'Network Choice 4',1,33121.00,49690.00,NULL,NULL,NULL,2785.00,1790.00,1790.00,3585.00,2785.00,1790.00,1790.00,3585.00),
	 (N'Network Choice 5',1,49691.00,NULL,NULL,NULL,NULL,3925.00,2395.00,2395.00,4855.00,3925.00,2395.00,2395.00,4855.00),
	 (N'First Choice 1',2,0.00,12810.00,NULL,NULL,NULL,1730.00,1045.00,NULL,NULL,1730.00,1045.00,NULL,NULL),
	 (N'First Choice 2 ',2,12811.00,24710.00,NULL,NULL,NULL,1730.00,1045.00,NULL,NULL,1730.00,1045.00,NULL,NULL),
	 (N'First Choice 3',2,24711.00,33120.00,NULL,NULL,NULL,4205.00,2435.00,NULL,NULL,4205.00,2435.00,NULL,NULL),
	 (N'First Choice 4',2,33121.00,49690.00,NULL,NULL,NULL,5315.00,3510.00,NULL,NULL,5315.00,3510.00,NULL,NULL),
	 (N'First Choice 5',2,49691.00,NULL,NULL,NULL,NULL,5875.00,3835.00,NULL,NULL,5875.00,3835.00,NULL,NULL);
INSERT INTO HRConnect.dbo.MedicalOptions (MedicalOptionName,MedicalOptionCategoryId,SalaryBracketMin,SalaryBracketMax,MonthlyMsaContributionAdult,MonthlyMsaContributionChild,MonthlyMsaContributionPrincipal,MonthlyRiskContributionAdult,MonthlyRiskContributionChild,MonthlyRiskContributionChild2,MonthlyRiskContributionPrincipal,TotalMonthlyContributionsAdult,TotalMonthlyContributionsChild,TotalMonthlyContributionsChild2,TotalMonthlyContributionsPrincipal) VALUES
	 (N'Essential Network 1',3,0.00,150230.00,520.00,315.00,655.00,2160.00,1265.00,NULL,2730.00,2680.00,1580.00,NULL,3385.00),
	 (N'Essential Network 2',3,150231.00,NULL,520.00,315.00,655.00,2620.00,1535.00,NULL,3310.00,3140.00,1850.00,NULL,3965.00),
	 (N'Essential Plus 1',3,0.00,150230.00,585.00,340.00,730.00,2410.00,1430.00,NULL,3055.00,2995.00,1770.00,NULL,3785.00),
	 (N'Essential Plus 2',3,150231.00,NULL,585.00,340.00,730.00,2955.00,1740.00,NULL,3740.00,3540.00,2080.00,NULL,4470.00),
	 (N'Vital Plus 1',4,0.00,60100.00,NULL,NULL,NULL,3740.00,1915.00,NULL,NULL,3740.00,1915.00,NULL,NULL),
	 (N'Vital Plus 2',4,60101.00,150230.00,NULL,NULL,NULL,4240.00,2160.00,NULL,NULL,4240.00,2160.00,NULL,NULL),
	 (N'Vital Plus 3',4,150231.00,NULL,NULL,NULL,NULL,4765.00,2435.00,NULL,NULL,4765.00,2435.00,NULL,NULL),
	 (N'Vital Network 1',5,0.00,60100.00,NULL,NULL,NULL,3445.00,1770.00,NULL,NULL,3445.00,1770.00,NULL,NULL),
	 (N'Vital Network 2',5,60101.00,150230.00,NULL,NULL,NULL,3920.00,2005.00,NULL,NULL,3920.00,2005.00,NULL,NULL),
	 (N'Vital Network 3',5,150231.00,NULL,NULL,NULL,NULL,4380.00,2245.00,NULL,NULL,4380.00,2245.00,NULL,NULL);
INSERT INTO HRConnect.dbo.MedicalOptions (MedicalOptionName,MedicalOptionCategoryId,SalaryBracketMin,SalaryBracketMax,MonthlyMsaContributionAdult,MonthlyMsaContributionChild,MonthlyMsaContributionPrincipal,MonthlyRiskContributionAdult,MonthlyRiskContributionChild,MonthlyRiskContributionChild2,MonthlyRiskContributionPrincipal,TotalMonthlyContributionsAdult,TotalMonthlyContributionsChild,TotalMonthlyContributionsChild2,TotalMonthlyContributionsPrincipal) VALUES
	 (N'Double Plus',5,0.00,NULL,515.00,270.00,NULL,6560.00,3705.00,NULL,NULL,7075.00,3975.00,NULL,NULL),
	 (N'Double Network',5,0.00,NULL,385.00,250.00,NULL,5840.00,3335.00,NULL,NULL,6225.00,3585.00,NULL,NULL),
	 (N'Alliance Plus',6,0.00,NULL,650.00,300.00,NULL,9885.00,5205.00,NULL,NULL,10535.00,5505.00,NULL,NULL),
	 (N'Alliance Network',6,0.00,NULL,590.00,270.00,NULL,8875.00,4690.00,NULL,NULL,9465.00,4960.00,NULL,NULL);
