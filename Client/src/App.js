import React, { useState, useEffect } from "react";
import { Routes, Route, useNavigate } from "react-router-dom";
import SignIn from "./Components/SignIn/SignIn";
import ForgotPassword from "./Components/ForgotPassword/ForgotPassword";
import AddEmployee from "./Components/AddEmployee";
import EditEmployee from "./Components/EditEmployee";
import MenuBar from "./Components/MenuBar";
import AddCompany from "./addCompany";
import EditCompany from "./Components/companyManagement/editCompany";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./App.css";
import "./MenuBar.css";
import EmployeeList from "./Components/EmployeeList";
import UserManagement from "./Components/UserManagement";
import PositionManagement from "./Components/PositionManagement";
import ViewPositionManagement from "./Components/ViewPositionManagement";
import TaxTableUpload from "./Components/TaxTableUpload";
import EditPositionManagement from "./Components/EditPositionManagement";
import AddPositionManagement from "./Components/AddPositionManagment";
import CompanyManagement from './companyManagement';
import CompanyContribution from './Components/CompanyContribution/CompanyContribution'; 
import Profile from './Components/MyProfile';
import CompensationPlanning from './Components/CompensationPlanning';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const handleBeforeUnload = () => {
      localStorage.removeItem("currentUser");
    };

  window.addEventListener("beforeunload", handleBeforeUnload);

  return () => {
    window.removeEventListener("beforeunload", handleBeforeUnload);
  };
}, []);



  useEffect(() => {
    const storedUser = localStorage.getItem("currentUser");

    if (storedUser && storedUser !== "undefined") {
      try {
        const userData = JSON.parse(storedUser);

        // Extract user object if nested
        if (userData.user) {
          setCurrentUser(userData.user);
        } else {
          setCurrentUser(userData);
        }
        setIsLoggedIn(true);
      } catch (e) {
        console.error("Failed to parse user data from localStorage", e);
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

const handleLoginSuccess = (responseData) => {
  console.log("User data received on login:", responseData);

  const user = responseData.user;  // extract nested user
  const token = responseData.token; // extract token

  setCurrentUser(user); // save in component state
  // Store both token and user data in localStorage with the key "currentUser"
  localStorage.setItem("currentUser", JSON.stringify({ token, user })); // persist in localStorage
  setIsLoggedIn(true);
  navigate("/dashboard");
};


  if (!isLoggedIn) {
    return (
      <div className="App">
        <Routes>
          <Route path="/" element={<SignIn onForgotPasswordClick={handleForgotPasswordClick} onLoginSuccess={handleLoginSuccess} />} />
          <Route path="/forgot-password" element={<ForgotPassword onBackToLogin={handleBackToLogin} />} />
        </Routes>
      </div>
    );
  }


 console.log("App currentUser:", currentUser);

 return (
 <div className="App" style={{ display: "flex", minHeight: "100vh" }}>
 <MenuBar currentUser={currentUser} />
 <div style={{ flex: 1, padding: "1rem" }}>
 <ToastContainer position="top-right" autoClose={3000} />
 <Routes>
 <Route path="/dashboard" element={<div>Welcome to Dashboard</div>} />
 <Route path="/addEmployee" element={<AddEmployee />} />
 <Route path="/editEmployee" element={<EditEmployee />} />
 <Route path="/editEmployee/:employeeNumber" element={<EditEmployee />} />
 <Route path="/addCompany" element={<AddCompany />} />
 <Route path="/companyManagement" element={<CompanyManagement/>} />
<Route path="/editCompany/:id" element={<EditCompany />} />
<Route path="/employeeList" element={<EmployeeList />} />
 <Route path="/company-contribution" element={<CompanyContribution />} />
 <Route path="/userManagement" element={<UserManagement />} /> 
 <Route path="/taxTableUpload" element={<TaxTableUpload />} />
 <Route path="/positionManagement" element={<PositionManagement />} />
<Route path="/addPositionManagement" element={<AddPositionManagement />} />
 <Route path="/editPositionManagement/:id" element={<EditPositionManagement />} />
 <Route path="/viewPositionManagement/:id" element={<ViewPositionManagement />} />
<Route
  path="/profile"
  element={<Profile currentUser={currentUser} />}
/>

<Route path="/company-contribution" element={<CompanyContribution />} />
<Route path="/compensationPlanning" element={<CompensationPlanning  />} />
 </Routes>

</div>
 </div>
);
}

export default App;