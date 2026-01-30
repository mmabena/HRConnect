const BASE_URL = "http://localhost:5037/api";

export const fetchUsersAndRoles = async () => {
  const token = localStorage.getItem("token");
  if (!token) throw new Error("No authentication token found");

  const headers = {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  };

  try {
    const [usersResponse, rolesResponse] = await Promise.all([
      fetch(`${BASE_URL}/user`, { headers }),
      fetch(`${BASE_URL}/roles`, { headers }),
    ]);

    if (!usersResponse.ok) {
      const errorMsg = await usersResponse.text();
      throw new Error(`Failed to fetch users: ${errorMsg}`);
    }

    if (!rolesResponse.ok) throw new Error(`Failed to fetch roles: ${rolesResponse.status}`);

    const users = await usersResponse.json();
    const roles = await rolesResponse.json();

    return { users, roles };
  } catch (error) {
    console.error("API Error:", error);
    throw error;
  }
};

export const updateUser = async (userId, userData) => {
  const token = localStorage.getItem("token");
  if (!token) throw new Error("No authentication token found");

  try {
    const response = await fetch(`${BASE_URL}/user/${userId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      },
      body: JSON.stringify(userData)
    });

    if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
    return await response.json();
  } catch (error) {
    console.error("Update User Error:", error);
    throw error;
  }
};
