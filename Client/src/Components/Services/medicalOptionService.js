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

export const getMedicalOptionsbyCategoryId = async (id) => {
  try{
      return await api.get(`${basePath}/${id}/category/options`);
  } catch (error)
  {
      throw error;
  }
};

export const getMedicalOptionsSalaryBracketMatchingEmployeeSalary = async (salaryAmount) => {
  try{
      return await api.get(`${basepath}/options/${salaryAmount}/salary-brakcet`);
  } catch (error) {
      throw error;
  }
};