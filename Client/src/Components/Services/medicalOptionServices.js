import api from '../../api/api';

const basePath = 'medical-options';

export const medicalOptionServics = {
  // GETS
  // Get Options grouped by Category
  getAllOptionsGroupedByCategory: async () => {
    //wrap around a try-catch block
    try{
      const response =  await api.get(`${basePath}/categories`);
      return response.data;
    }
    catch(error) {
      throw error;
    }
  },
  getAllMedicalOptionsCategories: async () =>
  {
    try{
      const response =  await api.get(`${basePath}/categories/all`);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  },
  getMedicalOptionsSnapshot: async () => {
    try{
      const response = await api.get(`${basePath}/snapshot`);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  },
  getCategoryById: async (id) => {
    try{
      const response = await api.get(`${basePath}/${id}/category`);
      return response.data;
    }
    catch(error) {
      throw error;
    }
  },
  getMedicalOptionsByCategoryId: async (id) => {
    try{
      const response = await api.get(`${basePath}/${id}/category/options`);
      return response.data;
    }
    catch (error){
      throw error;
    }
  },
  getMedicalOptionsSalaryBracketMatchingEmployeeSalary: async (salaryAmount) => {
    try{
      const response = await api.get(`${basePath}/options/${salaryAmount}/salary-brakcet`);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  },
  getMemberEligibilityOptionsByEmployeeId: async (employeeId) => {
    try{
      const response =  await api.get(`${basePath}/eligible/${employeeId}`);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  },
  // POSTS
  createMedicalOptionCategory: async (request) => {
    try{
      const response =  await api.post(`${basePath}/categories`, request);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  },
  createMedicalOptionCategoryOptionsByCategoryId: async (categoryId,request) => {
    try{
      const response = await api.post(`${basePath}/${categoryId}/category/options`, request);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  },
  //PUTS
  updateCategoryById: async (categoryId, request) => {
    try{
      const response = await api.put(`${basePath}/${categoryId}/category`, request);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  },
  updateBulkMedicalOptionsByCategoryId: async (categoryId, bulkRequest) => {
    try{
      const response = await api.put(`${basePath}/${categoryId}/variants`, bulkRequest);
      return response.data;
    }
    catch (error) {
      throw error;
    }
  }
};

export default medicalOptionServics;