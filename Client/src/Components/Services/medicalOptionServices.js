import api from '../../api/api';

const basePath = 'medical-options';

export const medicalOptionServics = {
  // GETS
  // Get Options grouped by Category
  getAllOptionsGroupedByCategory: async () => {
    const response =  await api.get(`${basePath}/categories`);
    return response.data;
  },
  getAllMedicalOptionsCategories: async () =>
  {
    const response =  await api.get(`${basePath}/categories/all`);
    return response.data;
  },
  getMedicalOptionsSnapshot: async () => {
    const response = await api.get(`${basePath}/snapshot`);
    return response.data;
  },
  getCategoryById: async (id) => {
    const response = await api.get(`${basePath}/${id}/category`);
    return response.data;
  },
  getMedicalOptionsByCategoryId: async (id) => {
    const response = await api.get(`${basePath}/${id}/category/options`);
    return response.data;
  },
  getMedicalOptionsSalaryBracketMatchingEmployeeSalary: async (salaryAmount) => {
    const response = await api.get(`${basePath}/options/${salaryAmount}/salary-brakcet`);
    return response.data;
  },
  getMemberEligibilityOptionsByEmployeeId: async (employeeId) => {
    const response =  await api.get(`${basePath}/eligible/${employeeId}`);
    return response.data;
  },
  // POSTS
  createMedicalOptionCategory: async (request) => {
    const response =  await api.post(`${basePath}/categories`, request);
    return response.data;
  },
  createBulkMedicalOptionCategoryOptionsByCategoryId: async (categoryId,request) => {
    const response = await api.post(`${basePath}/${categoryId}/category/options`, request);
    return response.data;
  },
  //PUTS
  updateCategoryById: async (categoryId, request) => {
    const response = await api.put(`${basePath}/${categoryId}/category`, request);
    return response.data;
  },
  updateBulkMedicalOptionsByCategoryId: async (categoryId, bulkRequest) => {
    return await api.put(`${basePath}/${categoryId}/variants`, bulkRequest);

  }
};

export default medicalOptionServics;