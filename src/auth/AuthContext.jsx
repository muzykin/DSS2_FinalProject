// src/auth/AuthContext.jsx
import React, { createContext, useState, useEffect, useContext } from "react";
import { api, setAuthToken } from "../api/client";

const AuthContext = createContext(null);

const TOKEN_KEY = "todo_token";
const USER_KEY = "todo_user";

function loadAuth() {
  const token = localStorage.getItem(TOKEN_KEY) || "";
  const userRaw = localStorage.getItem(USER_KEY) || "";
  const user = userRaw ? JSON.parse(userRaw) : null;
  return { token, user };
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => loadAuth().token);
  const [user, setUser] = useState(() => loadAuth().user);
  const [isReady, setReady] = useState(false);

  useEffect(() => {
    const { token: t, user: u } = loadAuth();
    setToken(t);
    setUser(u);
    setAuthToken(t);
    setReady(true);

    // Listen to storage events
    const onStorage = () => {
      const { token: t2, user: u2 } = loadAuth();
      setToken(t2);
      setUser(u2);
      setAuthToken(t2);
    };
    window.addEventListener('storage', onStorage);
    return () => window.removeEventListener('storage', onStorage);
  }, []);

  useEffect(() => {
    setAuthToken(token);
  }, [token]);

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setToken("");
    setUser(null);
    setAuthToken("");
  };

  const login = async (email, password) => {
    const { data } = await api.post("/api/auth/login", { email, password });
    localStorage.setItem(TOKEN_KEY, data.accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(data.user));
    setToken(data.accessToken);
    setUser(data.user);
    setAuthToken(data.accessToken);
  };

  const register = async (email, password, displayName) => {
    await api.post("/api/auth/register", { email, password, displayName });
    await login(email, password);
  };

  // Если ещё не готовы — ничего не рендерим (можно сделать лоадер!)
  if (!isReady) return null;

  return (
    <AuthContext.Provider value={{
      token,
      user,
      isAuthenticated: !!token,
      isReady,
      login,
      register,
      logout
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}