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