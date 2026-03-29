import React from "react";
import {
  Typography,
  Alert,
  CircularProgress,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Stack,
  Pagination,
  IconButton,
  Button,
  Chip
} from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import RadioButtonUncheckedIcon from "@mui/icons-material/RadioButtonUnchecked";
import { api, isApiError } from "../api/client";
import TodoDialog from "../components/TodoDialog";
import TodoFilters from "../components/TodoFilters";
import { useAuth } from "../auth/AuthContext";

const TodosPage = () => {
  const { token } = useAuth();
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState("");
  const [data, setData] = React.useState({
    items: [],
    page: 1,
    pageSize: 10,
    totalItems: 0,
    totalPages: 1
  });
  const [filters, setFilters] = React.useState({
    search: "",
    status: "all",
    priority: "",
    sortBy: "createdAt",
    sortDir: "desc"
  });
  const [dialog, setDialog] = React.useState({ open: false, mode: "create", todo: null });
  // Единственный источник текущей страницы:
  const [currentPage, setCurrentPage] = React.useState(1);

  // Получить список задач
  const fetchList = async (page = 1, externalFilters = filters) => {
    setLoading(true);
    setError("");
    try {
      const params = {
        page,
        pageSize: data.pageSize,
        search: externalFilters.search || undefined,
        status: externalFilters.status || undefined,
        priority: externalFilters.priority || undefined,
        sortBy: externalFilters.sortBy,
        sortDir: externalFilters.sortDir
      };
      const res = await api.get("/api/todos", { params });
      setData(res.data);
      setCurrentPage(page);
    } catch (err) {
      if (isApiError(err)) setError(err.response?.data?.title || "Failed to load todos");
      else setError("Failed to load todos");
    } finally {
      setLoading(false);
    }
  };

  // Сброс страницы на первую при изменении фильтров (как требует Cypress)
  React.useEffect(() => {
    if (!token) return;
    fetchList(1, filters);
    // eslint-disable-next-line
  }, [token, filters]);

  // Открытие/закрытие/создание/редактирование
  const openCreate = () => setDialog({ open: true, mode: "create", todo: null });
  const openEdit = (t) => setDialog({ open: true, mode: "edit", todo: t });
  const closeDialog = () => setDialog(p => ({ ...p, open: false }));

  // Применение фильтров с кнопки (исключительно если твой фильтр — с кнопкой "Применить")
  const handleApplyFilters = () => {
    setCurrentPage(1);
    fetchList(1, filters);
  };

  // Сохранить (после создания/редактирования — сбрасываем на первую)
  const saveTodo = async (payload) => {
    try {
      if (dialog.mode === "create") {
        await api.post("/api/todos", payload);
      } else {
        await api.put(`/api/todos/${dialog.todo.id}`, payload);
      }
      closeDialog();
      setCurrentPage(1);
      await fetchList(1, filters);
    } catch (err) {
      if (isApiError(err)) setError(err.response?.data?.title || "Save failed");
      else setError("Save failed");
    }
  };

  // Переключить завершенность — остаемся на текущей странице
  const toggleCompletion = async (t) => {
    try {
      await api.patch(`/api/todos/${t.id}/completion`, { isCompleted: !t.isCompleted });
      await fetchList(currentPage, filters);
    } catch (err) {
      if (isApiError(err)) setError(err.response?.data?.title || "Update failed");
      else setError("Update failed");
    }
  };

  // Удаление задачи — сбрасываем на первую
  const deleteTodo = async (t) => {
    if (!window.confirm(`Delete "${t.title}"?`)) return;
    try {
      await api.delete(`/api/todos/${t.id}`);
      setCurrentPage(1);
      await fetchList(1, filters);
    } catch (err) {
      if (isApiError(err)) setError(err.response?.data?.title || "Delete failed");
      else setError("Delete failed");
    }
  };

  // Меняем страницу через Pagination
  const handlePageChange = (_, p) => {
    setCurrentPage(p);
    fetchList(p, filters);
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">My Todos</Typography>
        <Button variant="contained" onClick={openCreate} data-cy="todo-create">
          Create
        </Button>
      </Stack>

      <TodoFilters value={filters} onChange={setFilters} onApply={handleApplyFilters} />

      {error ? <Alert severity="error" data-cy="todos-error">{error}</Alert> : null}

      <Paper sx={{ p: 2 }}>
        {loading ? <CircularProgress /> : (
          <>
            <Table size="small" data-cy="todos-table">
              <TableHead>
                <TableRow>
                  <TableCell>Title</TableCell>
                  <TableCell>Priority</TableCell>
                  <TableCell>Due</TableCell>
                  <TableCell>Public</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((t) => (
                  <TableRow key={t.id} data-cy={`todo-row-${t.id}`}>
                    <TableCell>
                      <Stack direction="row" spacing={1} alignItems="center">
                        <Typography variant="body2">{t.title}</Typography>
                        {t.isCompleted ? <Chip size="small" label="done" /> : null}
                      </Stack>
                    </TableCell>
                    <TableCell>{t.priority}</TableCell>
                    <TableCell>{t.dueDate || "-"}</TableCell>
                    <TableCell>{t.isPublic ? "yes" : "no"}</TableCell>
                    <TableCell>{t.isCompleted ? "completed" : "active"}</TableCell>
                    <TableCell align="right">
                      <IconButton onClick={() => toggleCompletion(t)} size="small" data-cy={`todo-toggle-${t.id}`}>
                        {t.isCompleted ? <RadioButtonUncheckedIcon /> : <CheckCircleIcon />}
                      </IconButton>
                      <IconButton onClick={() => openEdit(t)} size="small" data-cy={`todo-edit-${t.id}`}>
                        <EditIcon />
                      </IconButton>
                      <IconButton onClick={() => deleteTodo(t)} size="small" data-cy={`todo-delete-${t.id}`}>
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            <Stack direction="row" justifyContent="center" sx={{ mt: 2 }}>
              <Pagination
                count={data.totalPages || 1}
                page={currentPage}
                onChange={handlePageChange}
                data-cy="todos-pagination"
              />
            </Stack>
          </>
        )}
      </Paper>

      <TodoDialog
        open={dialog.open}
        mode={dialog.mode}
        initial={dialog.todo}
        onCancel={closeDialog}
        onSave={saveTodo}
      />
    </Stack>
  );
};

export default TodosPage;