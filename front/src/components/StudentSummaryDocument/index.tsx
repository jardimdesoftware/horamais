import { COLORS } from '@/config/colors';
import { Document, Page, StyleSheet, Text, View } from '@react-pdf/renderer';

export interface StudentSummaryActivity {
  nome: string;
  grupo: string;
  categoria: string;
  horasConcluidas: number;
  cargaMaximaCurso: number;
}

export interface StudentSummaryCertificate {
  titulo: string;
  instituicao: string;
  categoria: string;
  periodoLetivo: string;
  periodo: string;
  cargaHoraria: number;
}

export interface StudentSummaryData {
  aluno: {
    nome: string;
    matricula: string;
    email: string;
    situacao: string;
  };
  turma: {
    curso: string;
    codigo: string;
    periodo: string;
    turno: string;
  };
  horas: {
    complementarConcluidas: number;
    complementarExigidas: number;
    extensaoConcluidas: number;
    extensaoExigidas: number;
    possuiExtensao: boolean;
  };
  atividadesComplementares: StudentSummaryActivity[];
  atividadesExtensao: StudentSummaryActivity[];
  certificados: StudentSummaryCertificate[];
  coordenador: {
    nome: string;
    numeroPortaria: string;
    dou: string;
  } | null;
  emitidoEm: string;
}

const styles = StyleSheet.create({
  page: {
    paddingHorizontal: 40,
    paddingTop: 36,
    paddingBottom: 60,
    fontFamily: 'Helvetica',
    fontSize: 9,
    color: '#111111'
  },
  header: {
    borderBottomWidth: 2,
    borderBottomColor: COLORS.primary,
    paddingBottom: 8,
    marginBottom: 16
  },
  title: {
    fontFamily: 'Helvetica-Bold',
    fontSize: 14,
    color: COLORS.secondary
  },
  subtitle: {
    fontSize: 9,
    color: COLORS.text,
    marginTop: 3
  },
  section: {
    marginBottom: 14
  },
  sectionTitle: {
    fontFamily: 'Helvetica-Bold',
    fontSize: 10,
    color: COLORS.secondary,
    marginBottom: 6,
    textTransform: 'uppercase'
  },
  infoGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    borderWidth: 1,
    borderColor: '#D1D1D1',
    borderRadius: 3,
    padding: 8
  },
  infoItem: {
    width: '50%',
    paddingVertical: 3,
    paddingRight: 8
  },
  infoLabel: {
    fontSize: 7,
    color: COLORS.text,
    textTransform: 'uppercase',
    marginBottom: 2
  },
  infoValue: {
    fontSize: 9,
    fontFamily: 'Helvetica-Bold'
  },
  hoursRow: {
    flexDirection: 'row',
    gap: 10
  },
  hoursCard: {
    flex: 1,
    borderWidth: 1,
    borderColor: '#D1D1D1',
    borderRadius: 3,
    padding: 8
  },
  hoursCardTitle: {
    fontSize: 8,
    color: COLORS.text,
    textTransform: 'uppercase',
    marginBottom: 4
  },
  hoursCardValue: {
    fontFamily: 'Helvetica-Bold',
    fontSize: 13,
    color: COLORS.secondary
  },
  hoursCardCaption: {
    fontSize: 8,
    color: COLORS.text,
    marginTop: 2
  },
  tableHeader: {
    flexDirection: 'row',
    backgroundColor: '#F2F2F2',
    borderWidth: 1,
    borderColor: '#D1D1D1',
    paddingVertical: 4,
    paddingHorizontal: 6
  },
  tableRow: {
    flexDirection: 'row',
    borderLeftWidth: 1,
    borderRightWidth: 1,
    borderBottomWidth: 1,
    borderColor: '#D1D1D1',
    paddingVertical: 4,
    paddingHorizontal: 6
  },
  tableHeaderCell: {
    fontFamily: 'Helvetica-Bold',
    fontSize: 8,
    textTransform: 'uppercase'
  },
  tableCell: {
    fontSize: 8
  },
  alignRight: {
    textAlign: 'right'
  },
  emptyState: {
    borderWidth: 1,
    borderColor: '#D1D1D1',
    borderRadius: 3,
    padding: 8,
    fontSize: 8,
    color: COLORS.text
  },
  signature: {
    marginTop: 28,
    alignItems: 'center'
  },
  signatureLine: {
    borderTopWidth: 1,
    borderTopColor: '#111111',
    width: 260,
    paddingTop: 4,
    textAlign: 'center'
  },
  signatureName: {
    fontFamily: 'Helvetica-Bold',
    fontSize: 9
  },
  signatureRole: {
    fontSize: 8,
    color: COLORS.text,
    marginTop: 2
  },
  footer: {
    position: 'absolute',
    bottom: 28,
    left: 40,
    right: 40,
    flexDirection: 'row',
    justifyContent: 'space-between',
    borderTopWidth: 1,
    borderTopColor: '#D1D1D1',
    paddingTop: 6,
    fontSize: 7,
    color: COLORS.text
  }
});

const percentual = (concluidas: number, exigidas: number) =>
  exigidas > 0 ? Math.min((concluidas / exigidas) * 100, 100) : 0;

const InfoItem = ({ label, value }: { label: string; value: string }) => (
  <View style={styles.infoItem}>
    <Text style={styles.infoLabel}>{label}</Text>
    <Text style={styles.infoValue}>{value || '—'}</Text>
  </View>
);

const HoursCard = ({
  title,
  concluidas,
  exigidas
}: {
  title: string;
  concluidas: number;
  exigidas: number;
}) => (
  <View style={styles.hoursCard}>
    <Text style={styles.hoursCardTitle}>{title}</Text>
    <Text style={styles.hoursCardValue}>
      {concluidas}h de {exigidas}h
    </Text>
    <Text style={styles.hoursCardCaption}>
      {percentual(concluidas, exigidas).toFixed(0)}% concluído
      {concluidas < exigidas ? ` · faltam ${exigidas - concluidas}h` : ''}
    </Text>
  </View>
);

const ActivityTable = ({
  title,
  atividades
}: {
  title: string;
  atividades: StudentSummaryActivity[];
}) => (
  <View style={styles.section}>
    <Text style={styles.sectionTitle}>{title}</Text>
    {atividades.length === 0 ? (
      <Text style={styles.emptyState}>
        Nenhuma hora contabilizada nesta modalidade.
      </Text>
    ) : (
      <>
        <View style={styles.tableHeader}>
          <Text style={[styles.tableHeaderCell, { width: '30%' }]}>Grupo</Text>
          <Text style={[styles.tableHeaderCell, { width: '45%' }]}>
            Atividade
          </Text>
          <Text
            style={[
              styles.tableHeaderCell,
              styles.alignRight,
              { width: '12%' }
            ]}
          >
            Horas
          </Text>
          <Text
            style={[
              styles.tableHeaderCell,
              styles.alignRight,
              { width: '13%' }
            ]}
          >
            Limite
          </Text>
        </View>
        {atividades.map((atividade, index) => (
          <View
            key={`${atividade.grupo}-${atividade.nome}-${index}`}
            style={styles.tableRow}
            wrap={false}
          >
            <Text style={[styles.tableCell, { width: '30%' }]}>
              {atividade.grupo}
            </Text>
            <Text style={[styles.tableCell, { width: '45%' }]}>
              {atividade.nome}
            </Text>
            <Text
              style={[styles.tableCell, styles.alignRight, { width: '12%' }]}
            >
              {atividade.horasConcluidas}h
            </Text>
            <Text
              style={[styles.tableCell, styles.alignRight, { width: '13%' }]}
            >
              {atividade.cargaMaximaCurso}h
            </Text>
          </View>
        ))}
      </>
    )}
  </View>
);

export const StudentSummaryDocument = ({
  data
}: {
  data: StudentSummaryData;
}) => (
  <Document
    title={`Resumo de horas — ${data.aluno.nome}`}
    author={data.coordenador?.nome ?? 'Hora Mais'}
    subject={`Resumo de horas complementares de ${data.aluno.nome}`}
  >
    <Page size="A4" style={styles.page}>
      <View style={styles.header}>
        <Text style={styles.title}>Resumo de Horas do Estudante</Text>
        <Text style={styles.subtitle}>
          {data.turma.curso} · Turma {data.turma.codigo} · Emitido em{' '}
          {data.emitidoEm}
        </Text>
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Identificação</Text>
        <View style={styles.infoGrid}>
          <InfoItem label="Estudante" value={data.aluno.nome} />
          <InfoItem label="Matrícula" value={data.aluno.matricula} />
          <InfoItem label="E-mail" value={data.aluno.email} />
          <InfoItem label="Situação" value={data.aluno.situacao} />
          <InfoItem label="Curso" value={data.turma.curso} />
          <InfoItem
            label="Turma"
            value={`${data.turma.codigo} · ${data.turma.periodo} · ${data.turma.turno}`}
          />
        </View>
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Situação das horas</Text>
        <View style={styles.hoursRow}>
          <HoursCard
            title="Atividades complementares"
            concluidas={data.horas.complementarConcluidas}
            exigidas={data.horas.complementarExigidas}
          />
          {data.horas.possuiExtensao && (
            <HoursCard
              title="Atividades de extensão"
              concluidas={data.horas.extensaoConcluidas}
              exigidas={data.horas.extensaoExigidas}
            />
          )}
        </View>
      </View>

      <ActivityTable
        title="Horas complementares por atividade"
        atividades={data.atividadesComplementares}
      />

      {data.horas.possuiExtensao && (
        <ActivityTable
          title="Horas de extensão por atividade"
          atividades={data.atividadesExtensao}
        />
      )}

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Certificados aprovados</Text>
        {data.certificados.length === 0 ? (
          <Text style={styles.emptyState}>
            Nenhum certificado aprovado até a emissão deste resumo.
          </Text>
        ) : (
          <>
            <View style={styles.tableHeader}>
              <Text style={[styles.tableHeaderCell, { width: '28%' }]}>
                Atividade
              </Text>
              <Text style={[styles.tableHeaderCell, { width: '20%' }]}>
                Instituição
              </Text>
              <Text style={[styles.tableHeaderCell, { width: '16%' }]}>
                Categoria
              </Text>
              <Text style={[styles.tableHeaderCell, { width: '12%' }]}>
                Per. letivo
              </Text>
              <Text style={[styles.tableHeaderCell, { width: '14%' }]}>
                Período
              </Text>
              <Text
                style={[
                  styles.tableHeaderCell,
                  styles.alignRight,
                  { width: '10%' }
                ]}
              >
                Horas
              </Text>
            </View>
            {data.certificados.map((certificado, index) => (
              <View
                key={`${certificado.titulo}-${index}`}
                style={styles.tableRow}
                wrap={false}
              >
                <Text style={[styles.tableCell, { width: '28%' }]}>
                  {certificado.titulo}
                </Text>
                <Text style={[styles.tableCell, { width: '20%' }]}>
                  {certificado.instituicao}
                </Text>
                <Text style={[styles.tableCell, { width: '16%' }]}>
                  {certificado.categoria}
                </Text>
                <Text style={[styles.tableCell, { width: '12%' }]}>
                  {certificado.periodoLetivo}
                </Text>
                <Text style={[styles.tableCell, { width: '14%' }]}>
                  {certificado.periodo}
                </Text>
                <Text
                  style={[
                    styles.tableCell,
                    styles.alignRight,
                    { width: '10%' }
                  ]}
                >
                  {certificado.cargaHoraria}h
                </Text>
              </View>
            ))}
          </>
        )}
      </View>

      {data.coordenador && (
        <View style={styles.signature} wrap={false}>
          <View style={styles.signatureLine}>
            <Text style={styles.signatureName}>{data.coordenador.nome}</Text>
            <Text style={styles.signatureRole}>
              Coordenação de {data.turma.curso}
            </Text>
            {!!data.coordenador.numeroPortaria && (
              <Text style={styles.signatureRole}>
                Portaria nº {data.coordenador.numeroPortaria}
                {data.coordenador.dou ? ` · DOU ${data.coordenador.dou}` : ''}
              </Text>
            )}
          </View>
        </View>
      )}

      <View style={styles.footer} fixed>
        <Text>
          Documento gerado automaticamente pelo Hora Mais em {data.emitidoEm}.
        </Text>
        <Text
          render={({ pageNumber, totalPages }) =>
            `Página ${pageNumber} de ${totalPages}`
          }
        />
      </View>
    </Page>
  </Document>
);
