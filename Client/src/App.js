import React, { useState, useEffect } from "react";
import { Routes, Route, useNavigate } from "react-router-dom";
import SignIn from "./Components/SignIn/SignIn";
import ForgotPassword from "./Components/ForgotPassword/ForgotPassword";
import AddEmployee from "./Components/EmployeeManagement/AddEmployee";
import EditEmployee from "./Components/EmployeeManagement/EditEmployee";
import AddCompany from "./addCompany";
import EditCompany from "./Components/companyManagement/editCompany.jsx";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./App.css";
import "./Components/MenuBar/MenuBar.css";
import EmployeeList from "./Pages/EmployeeManagement/EmployeeList";
import AddEmployeeModal from "./Components/EmployeeManagement/AddEmployeeModal";
import UserManagement from "./Components/UserManagement";
import ViewPositionManagement from "./Components/ViewPositionManagement";
import TaxTableUpload from "./Components/companyManagement/TaxTableManagement/TaxTableUpload.jsx";
import EditPositionManagement from "./Components/CompanyManagement/PositionManagement/EditPositionManagement.jsx";
import AddPositionManagement from "./Components/CompanyManagement/PositionManagement/AddPositionManagment.jsx";
import PositionManagement from "./Pages/CompanyManagement/PositionManagement/PositionManagement";
import ChangePositionManagement from "./Components/CompanyManagement/PositionManagement/ChangePositionManagement.jsx";
import CompanyManagement from "./companyManagement";
import CompanyContribution from "./Components/CompanyContribution/CompanyContribution";
import Profile from "./Components/MyProfile";
import CompensationPlanning from "./Components/CompensationPlanning";
import TaxTableManagement from "./Components/companyManagement/TaxTableManagement/TaxTableManagement.jsx";
import ChangePassword from "./Components/ChangePassword";
import MenuBar from "./Components/MenuBar/MenuBar";
import ManageUserPositions from "./Pages/CompanyManagement/PositionManagement/ManageUserPositions.jsx";
import ProjectionCalculator from "./Pages/PayrollTools/ProjectionCalculator";
import PersonalInformation from "./Components/PersonalInformation.jsx";
import NotificationPage from "./Pages/NotificationPage/NotificationPage.jsx";
import api from "../src/api/api.js";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(() => {
    const token = localStorage.getItem("token");
    const storedUser = localStorage.getItem("currentUser");
    return !!token && !!storedUser;
  });
  const [currentUser, setCurrentUser] = useState(() => {
    const storedUser = localStorage.getItem("currentUser");
    return storedUser ? JSON.parse(storedUser) : null;
  });
  const navigate = useNavigate();

  //Load user from localStorage on refresh
  useEffect(() => {
    const fetchUserData = async () => {
      const token = localStorage.getItem("token");
      const storedUser = localStorage.getItem("currentUser");

      if (!token || !storedUser) return;

      try {
        const parsedUser = JSON.parse(storedUser);
        const email = parsedUser.email;

        const empResp = await api.get(`/employee/email/${email}`, {
          headers: { Authorization: `Bearer ${token}` },
        });

        const employee = empResp.data;

        const mergedUser = {
          ...parsedUser,
          username: `${employee.name} ${employee.surname}`,
          jobTitle: employee.positionTitle,
          employmentStatus: employee.employmentStatus,
          dateOfBirth: employee.dateOfBirth,
          profileImage: employee.profileImage,
        };

        setCurrentUser(mergedUser);
        localStorage.setItem("currentUser", JSON.stringify(mergedUser));
      } catch (error) {
        console.error("Failed to fetch employee:", error);
      }
    };

    fetchUserData();
  }, []);

  const handleForgotPasswordClick = () => {
    navigate("/forgot-password");
  };

  const handleBackToLogin = () => {
    navigate("/");
  };

  const handleLogout = () => {
    localStorage.removeItem("currentUser");
    setCurrentUser(null);
    setIsLoggedIn(false);
    navigate("/");
  };

  // FIXED: Use backend user object directly
  const handleLoginSuccess = async (backendUserData) => {
    try {
      const token = localStorage.getItem("token");

      let employee = null;

      try {
        const empResp = await api.get("/employee", {
          headers: { Authorization: `Bearer ${token}` },
        });

        employee = empResp.data.find(
          (emp) => emp.email === backendUserData.email,
        );
      } catch (err) {
        console.warn("Employee endpoint not accessible for this role");
      }

      const mergedUser = {
        ...backendUserData,
        username: employee
          ? `${employee.name} ${employee.surname}`
          : backendUserData.email,
        jobTitle: employee?.positionTitle || "NormalUser",
        employmentStatus: employee?.employmentStatus,
        dateOfBirth: employee?.dateOfBirth,
      };

      setCurrentUser(mergedUser);
      localStorage.setItem("currentUser", JSON.stringify(mergedUser));
      setIsLoggedIn(true);

      navigate("/dashboard");
    } catch (error) {
      console.error("Login error:", error);
    }
  };

  if (!isLoggedIn) {
    return (
      <div className="App">
        <Routes>
          <Route
            path="/"
            element={
              <SignIn
                onForgotPasswordClick={handleForgotPasswordClick}
                onLoginSuccess={handleLoginSuccess}
              />
            }
          />
          <Route
            path="/forgot-password"
            element={<ForgotPassword onBackToLogin={handleBackToLogin} />}
          />
        </Routes>
      </div>
    );
  }

  // console.log("App currentUser:", currentUser);

  return (
    <div className="App">
      <MenuBar currentUser={currentUser} onLogout={handleLogout} />
      <div>
        <ToastContainer position="top-right" autoClose={3000} />
        <Routes>
          <Route path="/dashboard" element={<div>Welcome to Dashboard</div>} />
          <Route path="/addEmployee" element={<AddEmployee />} />
          <Route path="/addEmployeeModal" element={<AddEmployeeModal />} />
          <Route path="/editEmployee" element={<EditEmployee />} />
          <Route
            path="/editEmployee/:employeeNumber"
            element={<EditEmployee />}
          />
          <Route path="/addCompany" element={<AddCompany />} />
          <Route path="/companyManagement" element={<CompanyManagement />} />
          <Route path="/editCompany/:id" element={<EditCompany />} />
          <Route path="/employeeList" element={<EmployeeList />} />
          <Route
            path="/company-contribution"
            element={<CompanyContribution />}
          />
          <Route path="/userManagement" element={<UserManagement />} />
          <Route path="/taxTableManagement" element={<TaxTableManagement />} />
          <Route path="/taxTableUpload" element={<TaxTableUpload />} />
          <Route path="/positionManagement" element={<PositionManagement />} />
          <Route
            path="/addPositionManagement"
            element={<AddPositionManagement />}
          />
          <Route
            path="/editPositionManagement/:id"
            element={<EditPositionManagement />}
          />
          <Route
            path="/viewPositionManagement/:id"
            element={<ViewPositionManagement />}
          />
          <Route
            path="/company-contribution"
            element={<CompanyContribution />}
          />
          <Route
            path="/compensationPlanning"
            element={<CompensationPlanning />}
          />
          <Route
            path="/changePassword"
            element={<ChangePassword currentUser={currentUser} />}
          />
          <Route
            path="/profile"
            element={<Profile currentUser={currentUser} />}
          />
          <Route
            path="/projection-calculator"
            element={<ProjectionCalculator />}
          />
          <Route path="/changeposition" element={<ChangePositionManagement />} />
          <Route path="/manageUserPosition" element={<ManageUserPositions/>} />
          <Route path="/personal" element={<PersonalInformation />} />
          <Route path="/notifications" element={<NotificationPage />} />
        </Routes>
      </div>
    </div>
  );
}

export default App;
