import axios from "axios";
import * as XLSX from "xlsx";
import { saveAs } from "file-saver";

const API_BASE_URL = "http://localhost:5037/api";

export const fetchLatestChange = async () => {
  try {
    const response = await axios.get(`${API_BASE_URL}/GroupLifeBenefitChangeHistory`);
    const changes = response.data;

    if (changes && changes.length > 0) {
      const sortedChanges = [...changes].sort(
        (a, b) => new Date(b.changeDate) - new Date(a.changeDate)
      );
      return sortedChanges[0];
    }
    return {
      newDeathBenefitPercentage: 0.565,
      newDisabilityCoverPercentage: 0.482,
    };
  } catch (error) {
    console.error("Error fetching latest change history:", error);
    throw error;
  }
};

export const downloadGroupLifeContributions = async () => {
  try {
    const response = await axios.get(`${API_BASE_URL}/GroupLifeContribution/all`);
    const data = response.data;
    const totalSum = data.reduce(
      (sum, item) => sum + (item.totalContribution || 0),
      0
    );

    const formattedData = data.map((item) => {
      const idOrPassport = item.idNumberOrPassport ? String(item.idNumberOrPassport).trim() : "";

      const isPassport = () => {
        if (!idOrPassport) return false;

        const isSAID = /^\d{13}$/.test(idOrPassport);
        const isSAPassport = /^[A-Za-z]\d{8}$/.test(idOrPassport) ||
          /^[A-Za-z]{2}\d{7}$/.test(idOrPassport);

        return !isSAID && isSAPassport;
      };

      const passportCheck = isPassport();

      return {
        employeeNumber: item.employeeNumber || "",
        firstName: item.firstName || "",
        lastName: item.lastName || "",
        gender: item.gender || "",
        dateOfBirth: item.dateOfBirth || "",
        idNumber: !passportCheck ? idOrPassport : "",
        passportNumber: passportCheck ? idOrPassport : "",
        contactNumber: item.contactNumber || "",
        emailAddress: item.emailAddress || "",
        deathBenefit: item.deathBenefit
          ? `R ${item.deathBenefit.toFixed(2)}`
          : "R 0.00",
        disabilityCover: item.disabilityCover
          ? `R ${item.disabilityCover.toFixed(2)}`
          : "R 0.00",
        totalContribution: item.totalContribution
          ? `R ${item.totalContribution.toFixed(2)}`
          : "R 0.00",
      };
    });

    const totalRow = {
      employeeNumber: "",
      firstName: "",
      lastName: "Total Group Life Contribution",
      gender: "",
      dateOfBirth: "",
      idNumber: "",
      passportNumber: "",
      contactNumber: "",
      emailAddress: "",
      deathBenefit: "",
      disabilityCover: "",
      totalContribution: `R ${totalSum.toFixed(2)}`,
    };

    const dataWithTotal = [...formattedData, totalRow];
    const worksheet = XLSX.utils.json_to_sheet(dataWithTotal);

    worksheet['!cols'] = [
      { width: 15 },  // employeeNumber
      { width: 20 },  // firstName
      { width: 20 },  // lastName
      { width: 10 },  // gender
      { width: 15 },  // dateOfBirth
      { width: 20 },  // idNumber
      { width: 20 },  // passportNumber
      { width: 15 },  // contactNumber
      { width: 25 },  // emailAddress
      { width: 15 },  // deathBenefit
      { width: 20 },  // disabilityCover
      { width: 20 }   // totalContribution
    ];
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, "GroupLife");

    const excelBuffer = XLSX.write(workbook, {
      bookType: "xlsx",
      type: "array",
    });
    const blob = new Blob([excelBuffer], {
      type: "application/octet-stream",
    });
    saveAs(blob, "GroupLifeContributions.xlsx");

    return true;
  } catch (error) {
    console.error("Error downloading spreadsheet:", error);
    throw error;
  }
};

export const updateBenefits = async (values) => {
  try {
    // Get the stored user data
    const userString = localStorage.getItem('user');
    
    if (!userString) {
      throw new Error("Please login again - no user data found");
    }

    const userData = JSON.parse(userString);
    
    // Extract needed values from the nested structure
    const token = userData.token;
    const employeeNumber = userData.user.employeeNumber;
    const userName = userData.user.userName;

    if (!token || !employeeNumber || !userName) {
      console.error('Missing required fields:', {
        tokenPresent: !!token,
        employeeNumberPresent: !!employeeNumber,
        userNamePresent: !!userName,
        fullUserData: userData
      });
      throw new Error("Session data incomplete. Please login again.");
    }

    const dto = {
      employeeNumber,
      changedBy: userName,
      newDeathBenefitPercentage: parseFloat(values.newDeathBenefitPercentage),
      newDisabilityCoverPercentage: parseFloat(values.newDisabilityCoverPercentage)
    };

    console.log("Sending update with:", dto);

    const response = await axios.post(
      `${API_BASE_URL}/GroupLifeBenefitChangeHistory/update-benefits`,
      dto,
      {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${token}`
        }
      }
    );

    return response.data;
  } catch (error) {
    console.error("Update failed with:", {
      error: error.message,
      localStorage: {
        user: localStorage.getItem('user'),
        token: localStorage.getItem('token')
      },
      stack: error.stack
    });
    throw error;
  }
};
