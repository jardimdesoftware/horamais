'use client';

import { signOut, useSession } from 'next-auth/react';
import { useEffect, useState } from 'react';
import { toast } from 'react-toastify';

import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';

import { extractApiError } from '@/lib/apiError';
import { CursoResponse, listarCursos } from '@/services/courseService';
import {
  assignCoordinator,
  createCoordinator,
  listUsers,
  ManagedUser,
  roleLabels,
  updateUser,
  UserPage
} from '@/services/userAdministrationService';
import { ChevronDown, Plus, Search, UserRoundCog, Users } from 'lucide-react';

type Change = { user: ManagedUser; kind: 'status' | 'role' };

export default function UsuariosPage() {
  const { data: session } = useSession();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [revision, setRevision] = useState(0);
  const [data, setData] = useState<UserPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [change, setChange] = useState<Change | null>(null);
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState('');
  const [coordinator, setCoordinator] = useState<ManagedUser | null>(null);
  const [newCoordinator, setNewCoordinator] = useState(false);
  const [coordinatorEmail, setCoordinatorEmail] = useState('');
  const [courses, setCourses] = useState<CursoResponse[]>([]);
  const [courseId, setCourseId] = useState('');
  const [coordinatorName, setCoordinatorName] = useState('');
  const [ordinance, setOrdinance] = useState('');
  const [dou, setDou] = useState('');

  useEffect(() => {
    listarCursos()
      .then(setCourses)
      .catch(() => toast.error('Não foi possível carregar os cursos.'));
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    const timer = setTimeout(async () => {
      setLoading(true);
      setError('');
      try {
        const result = await listUsers(search, page, controller.signal);
        if (!controller.signal.aborted) setData(result);
      } catch (err) {
        if (!controller.signal.aborted)
          setError(
            extractApiError(err, 'Não foi possível carregar os usuários.')
          );
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }, 250);
    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [search, page, revision]);

  const isSelf =
    change?.user.email.toLowerCase() === session?.user.email?.toLowerCase();
  const removingAdmin =
    change?.kind === 'role' && change.user.cargo === 'ADMIN';
  const actionLabel =
    change?.kind === 'status'
      ? change.user.ativo
        ? 'Desativar acesso'
        : 'Ativar acesso'
      : removingAdmin
        ? 'Remover cargo de administrador'
        : 'Promover a administrador';

  async function saveChange() {
    if (!change || saving) return;
    setSaving(true);
    setSaveError('');
    try {
      await updateUser(
        change.user.id,
        change.kind === 'status'
          ? { ativo: !change.user.ativo }
          : { administrador: change.user.cargo !== 'ADMIN' }
      );
      toast.success('Usuário atualizado.');
      if (isSelf) {
        await signOut({ callbackUrl: '/' });
        return;
      }
      setChange(null);
      setRevision((value) => value + 1);
    } catch (err) {
      setSaveError(
        extractApiError(err, 'Não foi possível atualizar o usuário.')
      );
    } finally {
      setSaving(false);
    }
  }

  function openChange(user: ManagedUser, kind: Change['kind']) {
    setSaveError('');
    setChange({ user, kind });
  }

  function openCoordinator(user: ManagedUser) {
    setSaveError('');
    setCoordinator(user);
    setCourseId(user.cursoId ?? '');
    setCoordinatorName(user.cargo === 'USUARIO' ? '' : user.nome);
    setOrdinance(user.numeroPortaria ?? '');
    setDou(user.dou ?? '');
  }

  function openNewCoordinator() {
    setSaveError('');
    setCoordinator(null);
    setNewCoordinator(true);
    setCoordinatorEmail('');
    setCourseId(
      new URLSearchParams(window.location.search).get('cursoId') ?? ''
    );
    setCoordinatorName('');
    setOrdinance('');
    setDou('');
  }

  async function saveCoordinator() {
    if ((!coordinator && !newCoordinator) || saving) return;
    setSaving(true);
    setSaveError('');
    try {
      const details = {
        cursoId: courseId,
        nome: coordinatorName.trim(),
        numeroPortaria: ordinance.trim(),
        dou: dou.trim()
      };
      if (newCoordinator)
        await createCoordinator({ ...details, email: coordinatorEmail.trim() });
      else await assignCoordinator(coordinator!.id, details);
      toast.success('Coordenação atribuída. A pessoa deverá entrar novamente.');
      setCoordinator(null);
      setNewCoordinator(false);
      setRevision((value) => value + 1);
    } catch (err) {
      setSaveError(
        extractApiError(err, 'Não foi possível atribuir a coordenação.')
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="mx-auto max-w-7xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="rounded-xl bg-blue-50 p-3 text-[#1351B4]">
            <Users aria-hidden="true" />
          </div>
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">
              Pessoas e acesso
            </h1>
          </div>
        </div>
        <Button onClick={openNewCoordinator}>
          <Plus className="mr-2 h-4 w-4" />
          Adicionar coordenador
        </Button>
      </div>

      <div className="rounded-xl border border-gray-200 bg-white">
        <div className="flex flex-wrap items-end justify-between gap-4 border-b p-5">
          <div className="w-full max-w-md">
            <label
              htmlFor="user-search"
              className="mb-2 block text-sm font-medium"
            >
              Buscar usuário
            </label>
            <div className="relative">
              <Search
                size={18}
                className="absolute left-3 top-3 text-gray-400"
                aria-hidden="true"
              />
              <Input
                id="user-search"
                className="pl-10"
                placeholder="Nome ou e-mail"
                value={search}
                onChange={(event) => {
                  setSearch(event.target.value);
                  setPage(1);
                }}
              />
            </div>
          </div>
          <p className="text-sm text-gray-500" aria-live="polite">
            {data ? `${data.total} usuário(s) encontrado(s)` : ''}
          </p>
        </div>

        {error ? (
          <div className="space-y-3 p-8 text-center" role="alert">
            <p className="text-red-700">{error}</p>
            <Button
              variant="outline"
              onClick={() => setRevision((value) => value + 1)}
            >
              Tentar novamente
            </Button>
          </div>
        ) : loading ? (
          <p className="p-12 text-center text-gray-500" role="status">
            Carregando usuários…
          </p>
        ) : !data?.items.length ? (
          <div className="p-12 text-center">
            <p className="font-medium">Nenhum usuário encontrado</p>
            <p className="mt-2 text-sm text-gray-500">
              Tente outro nome ou e-mail.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <caption className="sr-only">
                Usuários e permissões de acesso
              </caption>
              <thead className="bg-gray-50 text-gray-600">
                <tr>
                  {['Usuário', 'Cargo', 'Acesso', 'E-mail', 'Ações'].map(
                    (label) => (
                      <th
                        scope="col"
                        key={label}
                        className="px-5 py-3 font-medium"
                      >
                        {label}
                      </th>
                    )
                  )}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.items.map((user) => (
                  <tr key={user.id}>
                    <td className="px-5 py-4">
                      <p className="font-medium text-gray-900">
                        {user.nome}
                        {user.email.toLowerCase() ===
                          session?.user.email?.toLowerCase() && (
                          <span className="ml-2 text-xs text-blue-700">
                            Você
                          </span>
                        )}
                      </p>
                      <p className="mt-1 text-gray-500">{user.email}</p>
                    </td>
                    <td className="px-5 py-4">{roleLabels[user.cargo]}</td>
                    <td className="px-5 py-4">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${user.ativo ? 'bg-green-50 text-green-800' : 'bg-gray-100 text-gray-600'}`}
                      >
                        {user.ativo ? 'Ativo' : 'Inativo'}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-gray-600">
                      {user.emailVerificado ? 'Verificado' : 'Pendente'}
                    </td>
                    <td className="px-5 py-4">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button size="sm" variant="outline">
                            Gerenciar <ChevronDown className="ml-2 h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end" className="w-56">
                          {user.cargo !== 'ADMIN' && (
                            <DropdownMenuItem
                              disabled={
                                !user.ativo ||
                                !user.emailVerificado ||
                                !/@(discente\.)?ifpe\.edu\.br$/i.test(
                                  user.email
                                )
                              }
                              onSelect={() => openCoordinator(user)}
                            >
                              <UserRoundCog className="mr-2 h-4 w-4" />
                              {user.cargo === 'COORDENADOR'
                                ? 'Editar coordenação'
                                : 'Atribuir coordenação'}
                            </DropdownMenuItem>
                          )}
                          <DropdownMenuItem
                            disabled={
                              user.cargo !== 'ADMIN' &&
                              (!user.ativo ||
                                !user.emailVerificado ||
                                !/@(discente\.)?ifpe\.edu\.br$/i.test(
                                  user.email
                                ))
                            }
                            onSelect={() => openChange(user, 'role')}
                          >
                            {user.cargo === 'ADMIN'
                              ? 'Remover cargo ADMIN'
                              : 'Promover a ADMIN'}
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onSelect={() => openChange(user, 'status')}
                            className={user.ativo ? 'text-red-700' : ''}
                          >
                            {user.ativo ? 'Desativar acesso' : 'Ativar acesso'}
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <div className="flex items-center justify-between gap-3 border-t p-4">
          <Button
            variant="ghost"
            size="sm"
            disabled={loading || page <= 1}
            onClick={() => setPage((value) => value - 1)}
          >
            Anterior
          </Button>
          <span className="text-sm text-gray-500">
            Página {page} de {Math.max(1, Math.ceil((data?.total ?? 0) / 25))}
          </span>
          <Button
            variant="ghost"
            size="sm"
            disabled={loading || !data || page * data.pageSize >= data.total}
            onClick={() => setPage((value) => value + 1)}
          >
            Próxima
          </Button>
        </div>
      </div>

      <Dialog
        open={!!change}
        onOpenChange={(open) => {
          if (!open && !saving) setChange(null);
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{actionLabel}</DialogTitle>
            <DialogDescription>{change?.user.email}</DialogDescription>
          </DialogHeader>
          <p className="text-sm text-gray-600">
            {change?.kind === 'status'
              ? change.user.ativo
                ? 'O usuário perderá o acesso ao sistema, inclusive nas sessões atuais.'
                : 'O usuário poderá acessar novamente após autenticar sua conta.'
              : removingAdmin
                ? `O usuário passará a ${roleLabels[change!.user.cargoAposRebaixamento]} e perderá as permissões administrativas.`
                : 'O usuário poderá gerenciar o acesso de qualquer conta e os recursos administrativos do sistema.'}
          </p>
          <p className="text-sm text-gray-600">
            {isSelf
              ? 'Sua sessão será encerrada após a alteração.'
              : 'As sessões atuais desse usuário serão encerradas. Ele deverá entrar novamente.'}
          </p>
          {saveError && (
            <p className="text-sm text-red-700" role="alert">
              {saveError}
            </p>
          )}
          <DialogFooter>
            <Button
              variant="ghost"
              disabled={saving}
              onClick={() => setChange(null)}
            >
              Cancelar
            </Button>
            <Button disabled={saving} onClick={saveChange}>
              {saving ? 'Salvando…' : 'Confirmar alteração'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={!!coordinator || newCoordinator}
        onOpenChange={(open) => {
          if (!open && !saving) {
            setCoordinator(null);
            setNewCoordinator(false);
          }
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {coordinator?.cargo === 'COORDENADOR'
                ? 'Editar coordenação'
                : 'Atribuir coordenação'}
            </DialogTitle>
            <DialogDescription>
              {newCoordinator
                ? 'Informe o e-mail institucional. A pessoa entrará com Google.'
                : coordinator?.email}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-2">
            {newCoordinator && (
              <div>
                <label
                  htmlFor="coordinator-email"
                  className="mb-1 block text-sm font-medium"
                >
                  E-mail institucional
                </label>
                <Input
                  id="coordinator-email"
                  type="email"
                  value={coordinatorEmail}
                  onChange={(event) => setCoordinatorEmail(event.target.value)}
                  required
                />
              </div>
            )}
            <div>
              <label
                htmlFor="coordinator-name"
                className="mb-1 block text-sm font-medium"
              >
                Nome completo
              </label>
              <Input
                id="coordinator-name"
                value={coordinatorName}
                onChange={(event) => setCoordinatorName(event.target.value)}
                required
              />
            </div>
            <div>
              <label
                htmlFor="coordinator-course"
                className="mb-1 block text-sm font-medium"
              >
                Curso
              </label>
              <select
                id="coordinator-course"
                className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                value={courseId}
                onChange={(event) => setCourseId(event.target.value)}
                required
              >
                <option value="">Selecione um curso</option>
                {courses.map((course) => (
                  <option key={course.id} value={course.id}>
                    {course.nome}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label
                htmlFor="coordinator-ordinance"
                className="mb-1 block text-sm font-medium"
              >
                Número da portaria
              </label>
              <Input
                id="coordinator-ordinance"
                value={ordinance}
                onChange={(event) => setOrdinance(event.target.value)}
                required
              />
            </div>
            <div>
              <label
                htmlFor="coordinator-dou"
                className="mb-1 block text-sm font-medium"
              >
                DOU
              </label>
              <Input
                id="coordinator-dou"
                value={dou}
                onChange={(event) => setDou(event.target.value)}
                required
              />
            </div>
            <p className="text-sm text-gray-600">
              A sessão atual dessa pessoa será encerrada. O novo perfil valerá
              no próximo acesso.
            </p>
            {saveError && (
              <p className="text-sm text-red-700" role="alert">
                {saveError}
              </p>
            )}
          </div>
          <DialogFooter>
            <Button
              variant="ghost"
              disabled={saving}
              onClick={() => {
                setCoordinator(null);
                setNewCoordinator(false);
              }}
            >
              Cancelar
            </Button>
            <Button
              disabled={
                saving ||
                (newCoordinator &&
                  !/@(discente\.)?ifpe\.edu\.br$/i.test(
                    coordinatorEmail.trim()
                  )) ||
                !courseId ||
                !coordinatorName.trim() ||
                !ordinance.trim() ||
                !dou.trim()
              }
              onClick={saveCoordinator}
            >
              {saving ? 'Salvando…' : 'Salvar coordenação'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </section>
  );
}
