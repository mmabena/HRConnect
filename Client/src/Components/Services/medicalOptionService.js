import api from '../../api/api';

const basePath = 'medical-options';

// Get Options grouped by Category
export const getAllOptionsGroupedByCategory = async () =>
{
  //wrap around a try-catch block
  try{
      return await api.get(`${basePath}/categories`);
  } catch(error) {
      throw error;
  }
};

export const getAllMedicalOptionsCategories = async () =>
{
  try{
      return await api.get(`${basePath}/categories/all`);
  } catch (error) {
      throw error;
  }
};

export const getMedicalOptionsSnapshot = async () => {
  try{
      return await api.get(`${bashPath}/snapshot`);
  } catch (error) {
      throw error;
  }
};

export const getCategoryById = async (id) => {
  try{
      return await api.get(`${basePath}/${id}/category`);
  }  catch(error) {
      throw error;
  }
};

export const getMedicalOptionsByCategoryId = async (id) => {
  try{
      return await api.get(`${basePath}/${id}/category/options`);
  } catch (error)
  {
      throw error;
  }
};

export const getMedicalOptionsSalaryBracketMatchingEmployeeSalary = async (salaryAmount) => {
  try{
      return await api.get(`${basePath}/options/${salaryAmount}/salary-brakcet`);
  } catch (error) {
      throw error;
  }
};

export const getMemberEligibilityOptionsByEmployeeId = async (employeeId) => {
  try{
      return await api.get(`${basePath}/eligible/${employeeId}`);
  } catch (error) {
      throw error;
  }
};

// POSTS
export const createMedicalOptionCategory = async (request) => {
    try{
        return await api.post(`${basePath}/categories`, request);
    } catch (error) {
        throw error;
    }
};

export const createMedicalOptionCategoryOptionsByCategoryId = async (categoryId,request) => {
    try{
        return await api.post(`${basePath}/${categoryId}/category/options`, request);
    } catch (error) {
        throw error;
    }
};

// PUTS
export const updateCategoryById = async (categoryId, request) => {
    try{
        return await api.put(`${basePath}/${categoryId}/category`, request);
    } catch (error) {
        throw error;
    }
};

export const updateBulkMedicalOptionsByCategoryId = async (categoryId, bulkRequest) => {
    try{
        return await api.put(`${basePath}/${categoryId}/variants`, bulkRequest);
    } catch (error) {
        throw error;
    }
};