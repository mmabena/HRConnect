import React, { useState, useEffect } from "react";
import { Routes, Route, useNavigate } from "react-router-dom";
import SignIn from "./Components/SignIn/SignIn";
import ForgotPassword from "./Components/ForgotPassword/ForgotPassword";
import AddEmployee from "./Components/EmployeeManagement/AddEmployee";
import EditEmployee from "./Components/EmployeeManagement/EditEmployee";
import AddCompany from "./addCompany";
import EditCompany from "./Components/CompanyManagement/editCompany";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./App.css";
import UserManagement from "./Components/UserManagement";
import ViewPositionManagement from "./Components/ViewPositionManagement";
import EditPositionManagement from "./Components/CompanyManagement/PositionManagement/EditPositionManagement";
import AddPositionManagement from "./Components/CompanyManagement/PositionManagement/AddPositionManagment";
import PositionManagement from "./Pages/CompanyManagement/PositionManagement/PositionManagement";
import CompanyManagement from "./companyManagement";
import CompanyContribution from "./Components/CompanyContribution/CompanyContribution";
import Profile from "./Components/MyProfile";
import CompensationPlanning from "./Components/CompensationPlanning";
import TaxTableManagement from "./Components/CompanyManagement/TaxTableManagement/TaxTableManagement";
import ChangePassword from "./Components/ChangePassword";
import TaxTableUpload from "./Components/CompanyManagement/TaxTableManagement/TaxTableUpload";
import EmployeeList from "./Pages/EmployeeManagement/EmployeeList";
import MenuBar from "./Components/MenuBar/MenuBar";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const navigate = useNavigate();

  // ✅ Load user from localStorage on refresh
  useEffect(() => {
    const storedUser = localStorage.getItem("currentUser");

    if (storedUser) {
      try {
        const parsedUser = JSON.parse(storedUser);
        setCurrentUser(parsedUser);
        setIsLoggedIn(true);
      } catch {
        localStorage.removeItem("currentUser");
      }
    }
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
 const handleLoginSuccess = (userWithToken) => {
  setCurrentUser(userWithToken);
   localStorage.setItem("currentUser", JSON.stringify(userWithToken));
   console.log("App currentUser:", userWithToken);
  setIsLoggedIn(true);
  navigate("/dashboard");
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

  return (
    <div className="App">
      <MenuBar currentUser={currentUser} onLogout={handleLogout} />

      <div>
        <ToastContainer position="top-right" autoClose={3000} />

        <Routes>
          <Route path="/dashboard" element={<div>Welcome to Dashboard</div>} />

          <Route path="/addEmployee" element={<AddEmployee />} />
          <Route path="/editEmployee" element={<EditEmployee />} />
          <Route path="/editEmployee/:employeeNumber" element={<EditEmployee />} />

          <Route path="/addCompany" element={<AddCompany />} />
          <Route path="/companyManagement" element={<CompanyManagement />} />
          <Route path="/editCompany/:id" element={<EditCompany />} />

          <Route path="/employeeList" element={<EmployeeList />} />
          <Route path="/company-contribution" element={<CompanyContribution />} />
          <Route path="/userManagement" element={<UserManagement />}/>

          <Route path="/positionManagement" element={<PositionManagement />} />
          <Route path="/addPositionManagement" element={<AddPositionManagement />} />
          <Route path="/editPositionManagement/:id" element={<EditPositionManagement />} />
          <Route path="/viewPositionManagement/:id" element={<ViewPositionManagement />} />
          <Route path="/taxtablemanagement" element={<TaxTableManagement />} />
          <Route path="/profile" element={<Profile currentUser={currentUser} />} />
          <Route path="/compensationPlanning" element={<CompensationPlanning />} />
          <Route path="/changePassword" element={<ChangePassword currentUser={currentUser} />} />
          <Route path="/taxTableUpload" element={<TaxTableUpload />} />
        </Routes>
      </div>
    </div>
  );
}

export default App;