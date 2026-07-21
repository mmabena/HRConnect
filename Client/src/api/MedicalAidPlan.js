// src/api/MedicalAidPlan.js

import api from "./api";

export const getMedicalAidPlans = async () => {
  try {
    const response = await api.get("/medical-options/categories");

    const data = Array.isArray(response.data)
      ? response.data
      : response.data?.data || [];

    // ✅ DO NOT FLATTEN - return structure as-is but safe
    const cleaned = data.map((category) => ({
      medicalOptionCategoryId:
        category.medicalOptionCategoryId ?? category.categoryId ?? category.id,

      medicalOptionCategoryName:
        category.medicalOptionCategoryName ??
        category.categoryName ??
        category.name,

      medicalOptions: Array.isArray(category.medicalOptions)
        ? category.medicalOptions.map((opt) => ({
            medicalOptionId: opt.medicalOptionId,
            medicalOptionName: opt.medicalOptionName,
            medicalOptionCategoryId: opt.medicalOptionCategoryId,
            medicalOptionCategoryName: opt.medicalOptionCategoryName,
            salaryBracketMin: opt.salaryBracketMin,
            salaryBracketMax: opt.salaryBracketMax,

            // contributions (principal/adult/child)
            totalMonthlyContributionsPrincipal:
              opt.totalMonthlyContributionsPrincipal ?? 0,

            totalMonthlyContributionsAdult:
              opt.totalMonthlyContributionsAdult ?? 0,

            totalMonthlyContributionsChild:
              opt.totalMonthlyContributionsChild ?? 0,
          }))
        : [],
    }));

    return cleaned;
  } catch (error) {
    console.error("Error fetching medical aid plans:", error);
    return [];
  }
};

export const createMedicalAidDeduction = async (employeeId, payload) => {
  try {
    const response = await api.post(
      `/medical-aid-deductions/create/employee/${employeeId}`,
      payload,
    );
    return response.data;
  } catch (error) {
    console.error("Error creating medical aid deduction:", error);
    throw error;
  }
};

export const createMedicalAidDependent = async (employeeId, payload) => {
  try {
    const response = await api.post(
      `/medicalDependent/employee/${employeeId}`,
      payload,
    );
    return response.data;
  } catch (error) {
    console.error("Error creating medical aid dependent:", error);
    throw error;
  }
};

export const getEligibleMedicalAidPlans = async (payload) => {
  try {
    const response = await api.post(
      "/medical-aid-deductions/onboarding/eligible-options",
      payload,
    );

    const data = Array.isArray(response.data) ? response.data : [];

    const grouped = data.reduce((acc, plan) => {
      let category = acc.find(
        (c) => c.medicalOptionCategoryId === plan.medicalOptionCategoryId,
      );

      if (!category) {
        category = {
          medicalOptionCategoryId: plan.medicalOptionCategoryId,
          medicalOptionCategoryName: plan.medicalOptionCategoryName,
          medicalOptions: [],
        };

        acc.push(category);
      }

      category.medicalOptions.push({
        medicalOptionId: plan.medicalOptionId,
        medicalOptionName: plan.medicalOptionName,

        medicalOptionCategoryId: plan.medicalOptionCategoryId,
        medicalOptionCategoryName: plan.medicalOptionCategoryName,

        salaryBracketMin: plan.salaryBracketMin,
        salaryBracketMax: plan.salaryBracketMax,

        totalMonthlyContributionsPrincipal:
          plan.totalMonthlyContributionsPrincipal ?? 0,

        totalMonthlyContributionsAdult:
          plan.totalMonthlyContributionsAdult ?? 0,

        totalMonthlyContributionsChild:
          plan.totalMonthlyContributionsChild ?? 0,

        totalMonthlyContributionsSecondChild:
          plan.totalMonthlyContributionsChild2 ?? 0,

        estimatedTotalMonthlyPremium: plan.estimatedTotalMonthlyPremium ?? 0,
      });

      return acc;
    }, []);

    console.log("Eligible plans:", response.data);
    console.log("Is array?", Array.isArray(response.data));

    return grouped;
  } catch (error) {
    console.error("Error fetching eligible medical aid plans:", error);
    throw error;
  }
};
