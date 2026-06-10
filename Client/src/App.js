import React, { useState, useEffect } from "react";
import { Routes, Route, useNavigate } from "react-router-dom";
import SignIn from "./components/SignIn/SignIn.jsx";
import ForgotPassword from "./components/ForgotPassword/ForgotPassword.jsx";
import AddEmployee from "./components/EmployeeManagement/AddEmployee.jsx";
import EditEmployee from "./components/EmployeeManagement/EditEmployee.jsx";
import AddCompany from "./addCompany";
import EditCompany from "./components/companyManagement/editCompany.jsx";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./App.css";
import "./components/MenuBar/MenuBar.css";
import LeaveHistory from "./components/LeaveApplicationManagement/LeaveHistory.jsx";
import EmployeeList from "./Pages/EmployeeManagement/EmployeeList";
import Payslip from "./Pages/PayrollInfo/Payslip"
import AddEmployeeModal from "./components/EmployeeManagement/AddEmployeeModal.jsx";
import ViewPositionManagement from "./components/ViewPositionManagement.jsx";
import TaxTableUpload from "./components/companyManagement/TaxTableManagement/TaxTableUpload.jsx";
import EditPositionManagement from "./components/companyManagement/PositionManagement/EditPositionManagement.jsx";
import UserManagement from "./components/UserManagement/UserManagement.jsx";
import AddPositionManagement from "./components/companyManagement/PositionManagement/AddPositionManagment.jsx";
import PositionManagement from "./Pages/CompanyManagement/PositionManagement/PositionManagement";
import CompanyManagement from "./companyManagement";
import CompanyContribution from "./components/CompanyContribution/CompanyContribution.jsx";
import Profile from "./components/MyProfile.jsx";
import CompensationPlanning from "./components/CompensationPlanning.jsx";
import CompanyList from "./Pages/CompanyList.jsx"
import TaxTableManagement from "./components/companyManagement/TaxTableManagement/TaxTableManagement.jsx";
import ChangePassword from "./components/ChangePassword.jsx";
import MenuBar from "./components/MenuBar/MenuBar.jsx";
import ManageUserPositions from "./Pages/CompanyManagement/PositionManagement/ManageUserPositions.jsx";
import ProjectionCalculator from "./Pages/PayrollTools/ProjectionCalculator";
import PersonalInformation from "./components/PersonalInformation.jsx";
import NotificationPage from "./Pages/NotificationPage/NotificationPage.jsx";
import api from "../src/api/api.js";
import ChangePositionManagement from "./components/companyManagement/PositionManagement/ChangePositionManagement.jsx";
import LeaveTables from "./components/LeaveTypeManagement/LeaveTables.jsx";
import ApplyLeave from "./components/LeaveApplicationManagement/ApplyLeave.jsx";
import { resolveRole } from "./utils/roleUtils.js";
import AffectedEmployeesPage from "./components/LeaveTypeManagement/AffectedEmployeesPage.jsx";
import HomePage from "./Pages/HomePage/HomePage.jsx";

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

  const hideMenuBarRoutes = ["/companyManagement"];

  const shouldHideMenuBar = hideMenuBarRoutes.includes(
    window.location.pathname,
  );

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
        const resolvedRole=resolveRole(parsedUser?.User||parsedUser);

        const mergedUser = {
          ...parsedUser,
          role:resolveRole.roleName||parsedUser?.role,
          roleId:resolvedRole.roleId,
          username: `${employee.name} ${employee.surname}`,
          jobTitle: employee.positionTitle,
          employmentStatus: employee.employmentStatus,
          dateOfBirth: employee.dateOfBirth,
          profileImage: employee.profileImage,
        };
        //Store the current employee in the localStorage 
        localStorage.setItem("currentEmployee",JSON.stringify(employee));
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
    navigate("/login");
  };

  const handleLogout = () => {
    localStorage.removeItem("currentUser");
    setCurrentUser(null);
    setIsLoggedIn(false);
    navigate("/login");
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

      const role = resolveRole?.key ?? backendUserData?.role?.toLowerCase();

      if (role === "superuser") {
        navigate("/companyManagement");
      } else {
        navigate("/dashboard");
      }
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
            element= {
              <HomePage />
            }
          />
          <Route
            path="/login"
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
      {!shouldHideMenuBar && (
        <MenuBar currentUser={currentUser} onLogout={handleLogout} />
      )}
      <div>
        <ToastContainer position="top-right" autoClose={3000} />
        <Routes>
          <Route path="/" element={<HomePage />} />
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
          <Route path="/leaveManagement" element={<LeaveTables />} />
          <Route path="/companyManagement" element={<CompanyManagement />} />
          <Route path="/taxTableUpload" element={<TaxTableUpload />} />
          <Route path="/leave-application" element={<ApplyLeave />} />
          <Route path="/leave-history" element={<LeaveHistory />} />
          <Route path="/affected-employees" element={<AffectedEmployeesPage />}/>
          <Route path="/positionManagement" element={<PositionManagement />} />
          <Route path="/companyList" element={<CompanyList />} />
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
          <Route path="/changePositionManagement" element={<ChangePositionManagement />} />
          <Route path="/manageUserPosition" element={<ManageUserPositions />} />

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
          <Route path="/manageUserPosition" element={<ManageUserPositions />} />
          <Route path="/personal" element={<PersonalInformation />} />
          <Route path="/payslip" element= {<Payslip/>}/>
          <Route path="/notifications" element={<NotificationPage />} />
          {/* <Route path="/salarybenchmark" element={<SalaryBenchmark />} /> */}
        </Routes>
      </div>
    </div>
  );
}

export default App;

